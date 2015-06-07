using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Sensors;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.Devices.Notification;
using Windows.Phone.UI.Input;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace HungryBob
{
    public sealed partial class PlayArea : Page
    {
        // Sensor and dispatcher variables
        private Windows.Devices.Sensors.Accelerometer accelerometer;
        private double bobLocationX, bobLocationY, previousLocationX, previousLocationY;
        private double screenWidth, screenHeight;
        private bool gameRunning, gameOver;
        private int foodWidth, foodHeight;
        private double bobWidth, bobHeight;
        private double[] foodLocationX;
        private double[] foodLocationY;
        private int foodSpeed = 0;
        //private int enemySpeed = 0;
        private DispatcherTimer timer;
        private int numberOfFood;
        private Ellipse[] foodArray;
        private Random randomSeed;
        private Ellipse[] enemyArray;
        private int numberOfEnemies;
        private double[] enemyLocationX;
        private double[] enemyLocationY;
        private bool collideWithFood = false;
        private bool[] collideWithEnemy;
        private int foodEaten=0;
        private int enemyWidth, enemyHeight;
        private DisplayRequest KeepScreenOnRequest;
        private int foodToEat, strenghtGainRate, bobHealth;
        private float bobSpeed;
        private VibrationDevice vibrate;
        private int numberOfBricks;
        private Rectangle[] bricks;
        private double[] bricksLocX;
        private double[] bricksLocY;
        private int[] enemyDirection;
        private int[] enemySpeed;

        // animation variables
        private const int NumberOfColumns = 10;
        private const int NumberOfFrames = 10;
        private const double FrameWidth = 80;
        private const int FrameHeight = 64;
        private int currentFrame = 0;
        private bool bobMoveLeft = false;
        private bool bobLookingLeft = false;
        private int animationSpeed = 22;

        //Constructor
        public PlayArea()
        {
            KeepScreenOnRequest = new Windows.System.Display.DisplayRequest();
            this.InitializeComponent();
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            timeCounter();
            numberOfFood = 5;
            numberOfEnemies = 6;
            foodArray = new Ellipse[numberOfFood];
            foodLocationX = new double[numberOfFood];
            foodLocationY = new double[numberOfFood];
            randomSeed = new Random();
            accelerometer = Accelerometer.GetDefault();
            screenWidth = Window.Current.Bounds.Width;
            screenHeight = Window.Current.Bounds.Height;            
            bobWidth = Bob.Width;
            bobHeight = Bob.Height;            
            bobLocationX = screenWidth / 2 - bobWidth;
            bobLocationY = screenHeight / 2 - bobHeight;
            enemyArray = new Ellipse[numberOfEnemies];
            enemyLocationX = new double[numberOfEnemies]; 
            enemyLocationY = new double[numberOfEnemies];
            foodToEat = 11;
            bobSpeed = 2;
            strenghtGainRate = 1;
            bobHealth = 10;
            gameOver = false;
            numberOfBricks = 4;
            bricks = new Rectangle[numberOfBricks];
            bricksLocX = new double[numberOfBricks];
            bricksLocY = new double[numberOfBricks];
            collideWithEnemy = new bool[numberOfEnemies];
            enemyDeactivationTimerSetup();
            enemyDirection = new int[numberOfEnemies];
            enemySpeed = new int[numberOfEnemies];

            vibrate = VibrationDevice.GetDefault();

            ImageBrush foodImgBrush = new ImageBrush();
            foodImgBrush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/food.png"));
            ImageBrush enemyImgBrush = new ImageBrush();
            enemyImgBrush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/jellyfish.png"));

            
            for (int i = 0; i < foodArray.Length; i++)   
            {
                foodArray[i] = new Ellipse();
                foodArray[i].Width = 44;
                foodArray[i].Height = 33;
                //foodArray[i].Margin = new Thickness(10, 100 * i, 0, 0);
                foodArray[i].HorizontalAlignment = HorizontalAlignment.Left;
                foodArray[i].VerticalAlignment = VerticalAlignment.Top;
                foodArray[i].Visibility = Visibility.Visible;                               
                foodArray[i].Fill = foodImgBrush;
                foodGrid.Children.Add(foodArray[i]);
                foodLocationX[i] = randomSeed.Next(foodWidth, Convert.ToInt16(screenWidth) - foodWidth);
                foodLocationY[i] = randomSeed.Next(-Convert.ToInt16(screenHeight), 0);
                foodArray[i].Margin = new Thickness(foodLocationX[i], foodLocationY[i], 0, 0);
            }
            foodWidth = Convert.ToInt16(foodArray[0].Width);
            foodHeight = Convert.ToInt16(foodArray[0].Height);

            for (int i = 0; i < enemyArray.Length; i++)
            {
                collideWithEnemy[i] = false;
                enemyArray[i] = new Ellipse();
                enemyArray[i].Width = 55;
                enemyArray[i].Height = 55;
                //enemyArray[i].Margin = new Thickness(50*i, 10, 0, 0);
                enemyArray[i].HorizontalAlignment = HorizontalAlignment.Left;
                enemyArray[i].VerticalAlignment = VerticalAlignment.Top;
                enemyArray[i].Visibility = Visibility.Visible;
                enemyArray[i].Fill = enemyImgBrush;
                enemyDirection[i] = randomSeed.Next(0, 2);
                enemySpeed[i] = randomSeed.Next(1, 3);
                enemyGrid.Children.Add(enemyArray[i]);
                //enemyLocationX[i] = randomSeed.Next(0, Convert.ToInt16(screenWidth));
                //enemyGoesLeft = false;
                if (i % 2 == 0)
                {
                    enemyLocationX[i] = randomSeed.Next(-Convert.ToInt16(screenHeight), -55);
                }
                else
                {
                    enemyLocationX[i] = randomSeed.Next(Convert.ToInt16(screenWidth), Convert.ToInt16(screenWidth + 400));
                }                    
                enemyLocationY[i] = randomSeed.Next(0, Convert.ToInt16(screenHeight) - enemyHeight);
                
                enemyArray[i].Margin = new Thickness(enemyLocationX[i], enemyLocationY[i], 0, 0);
            }
            enemyWidth = Convert.ToInt16(enemyArray[0].Width);
            enemyHeight = Convert.ToInt16(enemyArray[0].Height);

            ImageBrush brickImgBrush = new ImageBrush();
            brickImgBrush.ImageSource = new BitmapImage(new Uri("ms-appx:///Assets/brick.png"));
            for (int i = 0; i < bricks.Length; i++)
            {
                bricks[i] = new Rectangle();
                bricks[i].Width = 66;
                bricks[i].Height = 33;
                bricks[i].HorizontalAlignment = HorizontalAlignment.Left;
                bricks[i].VerticalAlignment = VerticalAlignment.Top;
                bricks[i].Visibility = Visibility.Visible;
                bricks[i].Fill = brickImgBrush;
                enemyGrid.Children.Add(bricks[i]);
                bricksLocX[i] = randomSeed.Next(foodWidth, Convert.ToInt16(screenWidth) - foodWidth * 2);
                bricksLocY[i] = randomSeed.Next(-Convert.ToInt16(screenHeight), -foodHeight);
                bricks[i].Margin = new Thickness(bricksLocX[i], bricksLocY[i], 0, 0);
            }


            // I found how to use the accellerometer here https://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh465272
            if (accelerometer != null)
            {
                // Establish the report interval
                uint minReportInterval = accelerometer.MinimumReportInterval;
                //uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                accelerometer.ReportInterval = minReportInterval;

                // Assign an event handler for the reading-changed event
                accelerometer.ReadingChanged += new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
            }
        }       

        //This is executed every time the accelerometer reading is changed
        private async void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AccelerometerReading reading = e.Reading;

                previousLocationX = bobLocationX;
                previousLocationY = bobLocationY;

                if (gameRunning == true)
                {
                    bobLocationX += bobSpeed * -e.Reading.AccelerationY;

                    bobLocationY += (bobSpeed/2) * -e.Reading.AccelerationX;
                }

                //4 statements to check if the 4 screen borders have been reached
                if(bobLocationX < 0 )
                    bobLocationX = previousLocationX;

                if(bobLocationX > screenWidth - bobWidth)
                    bobLocationX = previousLocationX;

                if (bobLocationY < 0)
                    bobLocationY = previousLocationY;

                if (bobLocationY > screenHeight - bobHeight)
                    bobLocationY = previousLocationY;

                // Feeding the accelerometer readings into the player's location
                Bob.Margin = new Thickness(bobLocationX, bobLocationY, 0, 0);
                               
                if(gameRunning == true)
                {                    
                    moveFoodVertical();
                    collisionFood();
                    if (foodEaten > 17)
                    {                        
                        moveEnemyHorizontal();
                    }
                    collisionEnemy();
                    if (foodEaten > 50)
                    {
                        if(bricksComingMessage == false)                        
                            showNextStageMessage();
                        moveBricks();
                    }
                    collisionBricks();
                }                
            });
        }

        private void moveEnemyHorizontal()
        {
            for (int i = 0; i < enemyArray.Length; i++)
            {
                // if the enemy has left the screen, in the next appearence start if from a different
                // x position, different direciton and with different speed
                if ((enemyLocationX[i] >= screenWidth && enemyDirection[i] == 0) || (enemyLocationX[i] <= -enemyWidth && enemyDirection[i] == 1))
                {
                    enemyDirection[i] = randomSeed.Next(0, 2); // determine direction
                    enemySpeed[i] = randomSeed.Next(1, 3); // determine speed

                    if (enemyDirection[i] == 0)
                    {
                        enemyLocationX[i] = randomSeed.Next(-Convert.ToInt16(screenHeight), - enemyWidth);// go left to right
                    }
                    if (enemyDirection[i] == 1)
                    {
                        enemyLocationX[i] = randomSeed.Next(Convert.ToInt16(screenWidth), Convert.ToInt16(screenWidth + 400));// go right to left
                    }
                    enemyLocationY[i] = randomSeed.Next(0, Convert.ToInt16(screenHeight) - enemyHeight);
                }


                // if the enemy has not left the screen, keep moving it sideways
                if (enemyLocationX[i] < screenWidth && enemyDirection[i] == 0)
                {
                    enemyLocationX[i] += enemySpeed[i];
                }

                if (enemyLocationX[i] > -enemyWidth && enemyDirection[i] == 1)
                    enemyLocationX[i] -= enemySpeed[i];

                enemyArray[i].Margin = new Thickness(enemyLocationX[i], enemyLocationY[i], 0, 0);
            }
        }

        private void checkBobsDirection()
        {
            if (bobLocationX > previousLocationX)
            {
                bobMoveLeft = false;
            }                
            if (bobLocationX < previousLocationX)
            {                
                bobMoveLeft = true;
            }                
        }

        private void timeCounter()
        {
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 0, 0, 22); // this sets how often the timer should tick
            timer.Start(); 
            timer.Tick += timer_Tick;       
        }

        // this is executed every time the timer ticks
        void timer_Tick(object sender, object e)
        {
            if (gameRunning == true)
            {
                checkBobsDirection();
                if (bobMoveLeft == false)
                {
                    bobMovesRight();
                }
                if (bobMoveLeft == true)
                {
                    bobMovesLeft();
                }
            }
        }

        private void moveFoodVertical()
        {
            for (int i = 0; i < foodArray.Length; i++)
            {
                // if the food has left the screen, in the next appearence start if from a different y position
                if (foodLocationY[i] >= screenHeight)
                {
                    foodLocationX[i] = randomSeed.Next(foodWidth, Convert.ToInt16(screenWidth) - foodWidth);
                    foodLocationY[i] = randomSeed.Next(-Convert.ToInt16(screenHeight), 0);
                }

                // if the food has not left the screen, keep moving it down
                if (foodLocationY[i] < screenHeight)
                {
                    foodLocationY[i] += 0.5;
                }                
                    
                foodArray[i].Margin = new Thickness(foodLocationX[i], foodLocationY[i], 0, 0);
            }
        }

        private void moveBricks()
        {
            for (int i = 0; i < bricks.Length; i++)
            {
                if (bricksLocY[i] >= screenHeight)
                {
                    bricksLocX[i] = randomSeed.Next(foodWidth, Convert.ToInt16(screenWidth) - foodWidth * 2);
                    bricksLocY[i] = randomSeed.Next(-Convert.ToInt16(screenHeight), 0);
                }

                if (bricksLocY[i] < screenHeight)
                {
                    bricksLocY[i] += 1.5;
                }

                bricks[i].Margin = new Thickness(bricksLocX[i], bricksLocY[i], 0, 0);
            }
        }

        // I used this example to animate spritesheets http://www.spottedzebrasoftware.com/blog/xaml-spritesheet-animation.html
        private void animateBob(int spritesheetRow, float speed)
        {
            timer.Interval = new TimeSpan(0, 0, 0, 0, Convert.ToInt16(speed));
            currentFrame = (currentFrame + 1 + NumberOfFrames) % NumberOfFrames;
            var column = currentFrame % NumberOfColumns;
            var row = currentFrame / NumberOfColumns + spritesheetRow;

            bobMoving.X = -column * FrameWidth;
            bobMoving.Y = -row * FrameHeight;
        }

        private void bobMovesLeft()
        {            
            if(collideWithFood == true)
            {
                animateBob(3, animationSpeed);
                if (currentFrame == 8)
                    collideWithFood = false;
            }
            else if (bobLookingLeft == false)
            {               
                animateBob(5, animationSpeed);
                if (currentFrame == 8)
                    bobLookingLeft = true;
                //swimSound.Play();
            }
            else 
                animateBob(2, animationSpeed);
        }

        private void bobMovesRight()
        {
            if (collideWithFood == true)
            {
                animateBob(1, animationSpeed);
                if (currentFrame == 8)
                    collideWithFood = false;
            }
            else if (bobLookingLeft == true)
            {                
                animateBob(4, animationSpeed);               
                if (currentFrame == 8)
                    bobLookingLeft = false;
                //swimSound.Play();
            }
            else 
                animateBob(0, animationSpeed);  
        }
        
        private void collisionFood()
        {
            for(int i = 0; i < foodArray.Length; i++)
            {
                if(((foodLocationY[i] > bobLocationY) && ((foodLocationY[i] - foodWidth) < bobLocationY))
                && ((foodLocationX[i] > bobLocationX-33) && ((foodLocationX[i] - foodWidth-22) < bobLocationX)))
                {
                    collideWithFood = true;
                    foodEaten++;
                    textBox.Text = "= " + foodEaten.ToString();
                    foodLocationX[i] = screenWidth;
                    //eatingSound.Play();
                    if (bobSpeed <= 20)
                    {
                        bobSpeed ++;
                        animationSpeed --;
                    }
                    if (bobHealth <= 95)
                    {
                        bobHealth +=5;
                        healthLevel.Text = "=" + bobHealth.ToString() + "%";
                    }
                    if (bobHealth == 100 && jellyfishComingMessage == false)
                        showNextStageMessage();
                }        
            }            
        }

        private bool collideWithEnemyAny = false;
        private void collisionEnemy()
        {
            for (int i = 0; i < enemyArray.Length; i++)
            {
                if (((enemyLocationX[i] + enemyWidth/2 > bobLocationX) && (enemyLocationX[i] - enemyWidth < bobLocationX))
                && ((enemyLocationY[i] + enemyHeight > bobLocationY+11) && (enemyLocationY[i] - enemyHeight < bobLocationY)) 
                && collideWithEnemyAny == false)
                {
                    collideWithEnemyAny = true;
                    enemyDeactivationTimer.Start();
                    seconds = 0;
                    vibrate.Vibrate(TimeSpan.FromMilliseconds(200));
                    hurtSound.Play();
                    if (bobSpeed > 4)
                    {
                        bobSpeed -= 4;
                        animationSpeed += 4;
                    }
                    //if(bobHealth >= 10)
                        bobHealth -= 20;
                    healthLevel.Text = "=" + bobHealth.ToString() + "%";

                    if (bobHealth <= 0)
                    {
                        gameOver = true;
                        endGame();
                    }
                }
                //if (((enemyLocationX[i] + enemyWidth / 2 < bobLocationX) && (enemyLocationX[i] - enemyWidth > bobLocationX))
                //    && ((enemyLocationY[i] + enemyHeight < bobLocationY + 11) && (enemyLocationY[i] - enemyHeight > bobLocationY)))
                //{
                //    collideWithEnemy[i] = false;
                //}
            }
        }

        private void collisionBricks()
        {
            for (int i = 0; i < bricks.Length; i++)
            {
                if (((bricksLocX[i] + bricks[i].Width-11 > bobLocationX) && (bricksLocX[i] - bricks[i].Width < bobLocationX))
                && ((bricksLocY[i] + bricks[i].Height > bobLocationY + 11) && (bricksLocY[i] - bricks[i].Height < bobLocationY))
                && collideWithEnemyAny == false)
                {
                    collideWithEnemyAny = true;
                    enemyDeactivationTimer.Start();
                    seconds = 0;
                    vibrate.Vibrate(TimeSpan.FromMilliseconds(200));
                    hurtSound.Play();
                    if (bobSpeed > 6)
                    {
                        bobSpeed -= 6;
                        animationSpeed += 6;
                    }
                    //if(bobHealth >= 10)
                    bobHealth -= 30;
                    healthLevel.Text = "=" + bobHealth.ToString() + "%";

                    if (bobHealth <= 0)
                    {
                        gameOver = true;
                        endGame();
                    }
                }               
            }
        }

        private DispatcherTimer enemyDeactivationTimer;
        private int seconds = 0, timeEnemyDeactivated = 1;

        private void enemyDeactivationTimerSetup()
        {           
            enemyDeactivationTimer = new DispatcherTimer();
            enemyDeactivationTimer.Interval = new TimeSpan(0, 0, 1);
            enemyDeactivationTimer.Tick += enemyDeactivationTimer_Tick;            
        }

        void enemyDeactivationTimer_Tick(object sender, object e)
        {
            seconds ++;
            if(seconds == timeEnemyDeactivated)
            {
                collideWithEnemyAny = false;
                enemyDeactivationTimer.Stop();
            }
        }       

        private void endGame()
        {
            vibrate.Vibrate(TimeSpan.FromSeconds(1));
            PauseMenu.Visibility = Visibility.Collapsed;
            gameRunning = false;
            if(gameOver == true)
                gameOverGrid.Visibility = Visibility.Visible;
            gameOverScore.Text = "Score " + foodEaten; 
            
        }
       
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            backgroundMusic.Play();
            gameRunning = false;

            base.OnNavigatedTo(e);

            if (KeepScreenOnRequest == null)
                KeepScreenOnRequest = new DisplayRequest();

            KeepScreenOnRequest.RequestActive(); //  keep the screen on
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            KeepScreenOnRequest.RequestRelease();
        }

        //This brings up the pause menu
        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (PauseMenu.Visibility == Visibility.Collapsed)
            {
                PauseMenu.Visibility = Visibility.Visible;
                gameRunning = false;
            }
            else
            {
                PauseMenu.Visibility = Visibility.Collapsed;
                gameRunning = true;
            }
            e.Handled = true;
        }

        private void ResumeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {      
            //gameRunning = true;
            if (PauseMenu.Visibility == Visibility.Collapsed)
            {
                PauseMenu.Visibility = Visibility.Visible;
                gameRunning = false;
            }
            else
            {
                PauseMenu.Visibility = Visibility.Collapsed;
                gameRunning = true;
            }
        }

        //returning to the main menu
        private void MenuButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            gameRunning = false;
            timer.Stop();
            Frame.Navigate(typeof(MainPage));
        } 

        private void gameOverGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
        
        private void stageMessageGrid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            stageMessageGrid.Visibility = Visibility.Collapsed;
            gameRunning = true;
        }

        private bool jellyfishComingMessage = false, bricksComingMessage = false;

        private void showNextStageMessage()
        {
            gameRunning = false;
            stageMessageGrid.Visibility = Visibility.Visible;
            
            if (bobHealth == 100)
            {
                nextStageMessage.Text = "Well done! You have reached 100% health. \n Now watch out for jellyfish, because they are poisonous and will take 20% of Bob's health each time he touches them.";
                jellyfishComingMessage = true;
            }

            if (foodEaten > 50)
            {
                nextStageMessage.Text = "Naughty kids have come around and are throwing bricks in the water. Bob will lose 30% health each time he gets hit. \nWatch out!";
                bricksComingMessage = true;
            }
        }
    }
}
