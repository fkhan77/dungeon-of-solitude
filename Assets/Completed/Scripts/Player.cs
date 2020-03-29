using UnityEngine;
using UnityEngine.UI;	//Allows us to use UI.
using UnityEngine.SceneManagement;

namespace Completed
{
	//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
	public class Player : MovingObject
	{
		public float restartLevelDelay = 1f;		//Delay time in seconds to restart level.
		public int pointsPerFood = 10;				//Number of points to add to player health points when picking up a food object.
		public int pointsPerGoodFood = 20;		    //Number of points to add to player health points when picking up a good food object.
        public int pointsPerSmallGold = 10;         //Number of points to add to player gold points when picking up a smallGold object.
        public int pointsPerLargeGold = 15;         //Number of points to add to player gold points when picking up a largeGold object.
        public int pointsPerHugeGold = 25;          //Number of points to add to player gold points when picking up a HugeGold object.
        public int wallDamage = 1;					//How much damage a player does to a wall when attacking it.
		public Text healthText;						//UI Text to display current player health total.
        public Text goldText;                       //UI Text to display current player gold total
		public AudioClip moveSound1;				//1 of 2 Audio clips to play when player moves.
		public AudioClip moveSound2;				//2 of 2 Audio clips to play when player moves.
		public AudioClip eatSound1;					//1 of 4 Audio clips to play when player collects a food object.
		public AudioClip eatSound2;					//2 of 4 Audio clips to play when player collects a food object.
		public AudioClip eatsound3;				    //3 of 4 Audio clips to play when player collects a goodFood object.
		public AudioClip eatsound4;			     	//4 of 4 Audio clips to play when player collects a goodFood object.
        public AudioClip goldSound1;                //1 of 2 Audio clips to play when player collects a smallGold, largeGold or hugeGold object
        public AudioClip goldSound2;                //1 of 2 Audio clips to play when player collects a smallGold, largeGold or hugeGold object
        public AudioClip gameOverSound;				//Audio clip to play when player dies.
		
		private Animator animator;					//Used to store a reference to the Player's animator component.
		private int health;							//Used to store player health points total during level.
        private int gold;                           //Used to store player gold points total during level

        public object LoadedSceneMode { get; private set; }


        //Start overrides the Start function of MovingObject
        protected override void Start ()
		{
			//Get a component reference to the Player's animator component
			animator = GetComponent<Animator>();
			
			//Get the current health point total stored in GameManager.instance between levels.
			health = GameManager.instance.playerHealthPoints;

            //Get the current gold point toal stored in GameManager.instance between levels.
            gold = GameManager.instance.playerGoldPoints;

            //Set the healthText to reflect the current player health total.
            healthText.text = "Health: " + health;

            //Set the goldText tor reflect the current player gold total.
            goldText.text = "Gold:" + gold;

			//Call the Start function of the MovingObject base class.
			base.Start ();
		}
		
		
		//This function is called when the behaviour becomes disabled or inactive.
		private void OnDisable ()
		{
			//When Player object is disabled, store the current local health & gold total in the GameManager so it can be re-loaded in next level.
			GameManager.instance.playerHealthPoints = health;
            GameManager.instance.playerGoldPoints = gold;
		}
		
		
		private void Update ()
		{
			//If it's not the player's turn, exit the function.
			if(!GameManager.instance.playersTurn) return;
			
			int horizontal = 0;  	//Used to store the horizontal move direction.
			int vertical = 0;		//Used to store the vertical move direction.
			
			
			//Get input from the input manager, round it to an integer and store in horizontal to set x axis move direction
			horizontal = (int) (Input.GetAxisRaw ("Horizontal"));
			
			//Get input from the input manager, round it to an integer and store in vertical to set y axis move direction
			vertical = (int) (Input.GetAxisRaw ("Vertical"));
			
			//Check if moving horizontally, if so set vertical to zero.
			if(horizontal != 0)
			{
				vertical = 0;
			}
			
			//Check if we have a non-zero value for horizontal or vertical
			if(horizontal != 0 || vertical != 0)
			{
				//Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one.
				//Pass in horizontal and vertical as parameters to specify the direction to move Player in.
				AttemptMove<Wall> (horizontal, vertical);
			}
		}
		
		//AttemptMove overrides the AttemptMove function in the base class MovingObject
		//AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
		protected override void AttemptMove <T> (int xDir, int yDir)
		{
			//Every time player moves, subtract 1 from health points total.
			health--;
			
			//Update health text display to reflect current health.
			healthText.text = "Health: " + health;

            //Update gold text display to reflect current score
            goldText.text = "Gold: " + gold;

            //Call the AttemptMove method of the base class.
            base.AttemptMove <T> (xDir, yDir);
			
			//Hit allows us to reference the result of the Linecast done in Move.
			RaycastHit2D hit;
			
			//If Move returns true, meaning Player was able to move into an empty space.
			if (Move (xDir, yDir, out hit)) 
			{
				//Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
				SoundManager.instance.RandomizeSfx (moveSound1, moveSound2);
			}
			
			//Since the player has moved and lost health points, check if the game has ended.
			CheckIfGameOver ();
			
			//Set the playersTurn boolean of GameManager to false now that players turn is over.
			GameManager.instance.playersTurn = false;
		}
		
		
		//OnCantMove overrides the abstract function OnCantMove in MovingObject.
		//It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
		protected override void OnCantMove <T> (T component)
		{
			//Set hitWall to equal the component passed in as a parameter.
			Wall hitWall = component as Wall;
			
			//Call the DamageWall function of the Wall we are hitting.
			hitWall.DamageWall (wallDamage);
			
			//Set the attack trigger of the player's animation controller in order to play the player's attack animation.
			animator.SetTrigger ("playerChop");
		}
		
		
		//OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
		private void OnTriggerEnter2D (Collider2D other)
		{
            //Check if the tag of the trigger collided with is Exit.
            if (other.tag == "Exit")
            {
                //Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
                Invoke("Restart", restartLevelDelay);

                //Disable the player object since level is over.
                enabled = false;
            }

            //Check if the tag of the trigger collided with is Food.
            else if (other.tag == "Food")
            {
                //Add pointsPerFood to the players current health total.
                health += pointsPerFood;

                //Update healthText to represent current health total and notify player that they gained points
                healthText.text = "+" + pointsPerFood + " Health: " + health;

                //Call the RandomizeSfx function of SoundManager and pass in two eating sounds to choose between to play the eating sound effect.
                SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);

                //Disable the food object the player collided with.
                other.gameObject.SetActive(false);
            }

            //Check if the tag of the trigger collided with is goodFood.
            else if (other.tag == "goodFood")
            {
                //Add pointsPerGoodFood to players health points total.
                health += pointsPerGoodFood;

                //Update healthText to represent current health total and notify player that they gained health points.
                healthText.text = "+" + pointsPerGoodFood + " Health: " + health;

                //Call the RandomizeSfx function of SoundManager and pass in two eating sounds to choose between to play the eating sound effect.
                SoundManager.instance.RandomizeSfx(eatsound3, eatsound4);

                //Disable the goodFood object the player collided with.
                other.gameObject.SetActive(false);
            }
            //Check if the tag of the trigger collided with is smallGold.
            else if (other.tag == "smallGold")
            {
                //Add pointsPerSmallGold to players gold points total.
                gold += pointsPerSmallGold;

                //Update goldText to represent current gold total and notify player that they gained gold points.
                goldText.text = "+" + pointsPerSmallGold + " Gold:" + gold;

                //Update playerGoldpoints so when GameOver function is called the correct amount of gold is shown.
                GameManager.instance.playerGoldPoints = gold; 
                
                //Call the RandomizeSfx function of SoundManager and pass in two coin sounds to choose between to play the coing sound effect.
                SoundManager.instance.RandomizeSfx(goldSound1, goldSound2); 

                //Disable the smallGold object the player collided with.
                other.gameObject.SetActive(false);

            }
            //Check if the tag of the trigger collided with is largeGold.
            else if (other.tag == "largeGold")
            {
                //Add pointsPerLargeGold to players gold points total.
                gold += pointsPerLargeGold;

                //Update goldText to represent current gold total and notify player that they gained gold points.
                goldText.text = "+" + pointsPerLargeGold + " Gold:" + gold;

                //Update playerGoldpoints so when GameOver function is called the correct amount of gold is shown.
                GameManager.instance.playerGoldPoints = gold;

                //Call the RandomizeSfx function of SoundManager and pass in two coin sounds to choose between to play the coing sound effect.
                SoundManager.instance.RandomizeSfx(goldSound1, goldSound2);

                //Disable the smallGold object the player collided with.
                other.gameObject.SetActive(false);

            }
            //Check if the tag of the trigger collided with is hugeGold.
            else if (other.tag == "hugeGold")
            {
                //Add pointsPerHugeGold to players gold points total.
                gold += pointsPerHugeGold;

                //Update goldText to represent current gold total and notify player that they gained gold points.
                goldText.text = "+" + pointsPerHugeGold + " Gold:" + gold;

                //Update playerGoldpoints so when GameOver function is called the correct amount of gold is shown.
                GameManager.instance.playerGoldPoints = gold;

                //Call the RandomizeSfx function of SoundManager and pass in two coin sounds to choose between to play the coing sound effect.
                SoundManager.instance.RandomizeSfx(goldSound1, goldSound2);

                //Disable the smallGold object the player collided with.
                other.gameObject.SetActive(false);

            }

        }
		
		
		//Restart reloads the scene when called.
		private void Restart ()
		{
            //Load the last scene loaded, in this case Main, the only scene in the game.
            SceneManager.LoadScene(0);
		}
		
		
		//LoseHealth is called when an enemy attacks the player.
		//It takes a parameter loss which specifies how many health points to lose.
		public void LoseHealth (int loss)
		{
			//Set the trigger for the player animator to transition to the playerHit animation.
			animator.SetTrigger ("playerHit");
			
			//Subtract lost health points from the players total.
			health -= loss;
			
			//Update the health display with the new total.
			healthText.text = "-"+ loss + " Health: " + health;
			
			//Check to see if game has ended.
			CheckIfGameOver ();
		}
		
		
		//CheckIfGameOver checks if the player is out of health points and if so, ends the game.
		private void CheckIfGameOver ()
		{
			//Check if health point total is less than or equal to zero.
			if (health <= 0) 
			{
				//Call the PlaySingle function of SoundManager and pass it the gameOverSound as the audio clip to play.
				SoundManager.instance.PlaySingle (gameOverSound);
				
				//Stop the background music.
				SoundManager.instance.musicSource.Stop();
				
				//Call the GameOver function of GameManager.
				GameManager.instance.GameOver ();
			}
		}
	}
}

