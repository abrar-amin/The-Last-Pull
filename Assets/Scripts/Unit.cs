using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Unit : MonoBehaviour
{


	//Unit should also have all "Battle Station" code here. 
	public Dictionary<string, Move> Moves = new Dictionary<string, Move>();
	public BattleHUD UnitHud;


	public string unitName;
	public int unitLevel;

	public int damage;

	public int maxHP;
	public int currentHP;




	public bool AugmentHealth(int amount)
	{
		currentHP += amount;

		if (currentHP <= 0)
			return true;

		if(currentHP > maxHP)
        {
			currentHP = maxHP; 
        }

		return false;
	}


	public bool Heal(int amount)
	{
		currentHP += amount;


		if (currentHP > maxHP)
		{
			currentHP = maxHP;
		}

		return false;
	}


	public bool TakeDamage(int damage)
	{
		currentHP -= damage;


		if (currentHP <= 0)
		{
			return true;
		}

		return false;
	}

	public int PerformMove(string moveName, ref Unit target)
	{
		if(!Moves.ContainsKey(moveName))
        {
			Debug.Log("Move does not exist!");
			return -1;
        }


		if (this == target)
		{
			Debug.Log("This is a reflexive move.");
		}
		else
		{
			Debug.Log("This is a targeted move.");
		}

		Move ChosenMove = Moves[moveName];
		GameObject MoveCopy = (GameObject)Instantiate(Resources.Load<GameObject>("Prefabs/MyPrefab"));

		//To Do: Encapsulate all dialogue data into Unit class.
			//dialogueText.text = MoveCopy.FlavorText;

		Destroy(MoveCopy, ChosenMove.MoveDuration);

		return ChosenMove.Damage;
	}
}
