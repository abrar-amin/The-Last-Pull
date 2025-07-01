using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BattleState { START, PLAYERTURN, ENEMYTURN, WON, LOST }
public class BattleSystem : MonoBehaviour
{

	public int[] playerMoveDamage = { 5, 7, 15, 19 };



	public GameObject playerPrefab;
	public GameObject enemyPrefab;

	public GameObject playerGO;
	public GameObject enemyGO;

	public Animator playerAnimator;
	public Animator enemyAnimator;

	public Transform playerBattleStation;
	public Transform enemyBattleStation;

	public ThreeReelController reelControllerScript;

	public GameObject creditPrefab;
	public GameObject whalePrefab;
	public GameObject charmPrefab;
	public GameObject moneyPrefab;
	public GameObject healPrefab;
	public GameObject titleScreen;

	Unit playerUnit;
	Unit enemyUnit;

	public Text dialogueText;

	public BattleHUD playerHUD;
	public BattleHUD enemyHUD;

	public BattleState state;
	public ThreeReelController HandleScript;
    // Start is called before the first frame update
    void Start()
    {

		state = BattleState.START;
		StartCoroutine(SetupBattle());
    }

	IEnumerator SetupBattle()
	{
		playerGO = Instantiate(playerPrefab, playerBattleStation);
		playerUnit = playerGO.GetComponent<Unit>();

		enemyGO = Instantiate(enemyPrefab, enemyBattleStation);
		enemyUnit = enemyGO.GetComponent<Unit>();

		playerAnimator = playerGO.GetComponent<Animator>();
		enemyAnimator = enemyGO.GetComponent<Animator>();

		reelControllerScript.canSpin = false;

		dialogueText.text = "A wild " + enemyUnit.unitName + " approaches...";

		playerHUD.SetHUD(playerUnit);
		enemyHUD.SetHUD(enemyUnit);

		yield return new WaitForSeconds(7f);
		Destroy(titleScreen);
		PlayerTurn();
	}

	IEnumerator PlayerAttack()
	{

		bool isDead = enemyUnit.TakeDamage(playerUnit.damage);

		enemyHUD.SetHP(enemyUnit.currentHP);
		dialogueText.text = "The attack is successful!";

		yield return new WaitForSeconds(2f);

		if(isDead)
		{
			state = BattleState.WON;
			EndBattle();
		} else
		{
			state = BattleState.ENEMYTURN;
			StartCoroutine(EnemyTurn());
		}
	}

	IEnumerator PlayerAttackReel()
	{
		yield return new WaitForSeconds(8f);
		bool heal = false;
		if (reelControllerScript.leftReelIndex == reelControllerScript.rightReelIndex && reelControllerScript.leftReelIndex == reelControllerScript.middleReelIndex)
		{
			//Credit Card!
			// Switch statement
			if (reelControllerScript.leftReelIndex == 0)
			{
				GameObject creditCopy = Instantiate(creditPrefab);
				dialogueText.text = "Get Credit Carded!!";

				Destroy(creditCopy, 1.5f);
			}
			//Whaling!
			else if (reelControllerScript.leftReelIndex == 1)
			{
				GameObject whaleCopy = Instantiate(whalePrefab);
				dialogueText.text = "Get Whaled!!!!!!!!";

				Destroy(whaleCopy, 1.5f);
			}
			else
            {
				heal = true;
				GameObject healCopy = Instantiate(healPrefab);
				dialogueText.text = "Heal yourself of your emotional pains!";
				Destroy(healCopy, 1.5f);
				yield return new WaitForSeconds(2f);

				playerUnit.Heal(10);
				playerHUD.SetHP(playerUnit.currentHP);
				yield return new WaitForSeconds(2f);

			}
			bool isDead = false; 
			if(!heal)
            {
				isDead = enemyUnit.TakeDamage(playerMoveDamage[reelControllerScript.leftReelIndex + 1]);
				enemyHUD.SetHP(enemyUnit.currentHP);
				if (isDead)
				{
					state = BattleState.WON;
					EndBattle();

				}
			}

			if(!isDead)
            {
				//Wait for move to finish for a bit
				yield return new WaitForSeconds(2f);

				state = BattleState.ENEMYTURN;
				StartCoroutine(EnemyTurn());

			}

		}
		// Two Match (Money) 
		else if (reelControllerScript.leftReelIndex == reelControllerScript.rightReelIndex  || reelControllerScript.leftReelIndex  == reelControllerScript.middleReelIndex || reelControllerScript.rightReelIndex == reelControllerScript.middleReelIndex)
		{
			//Credit Card!
			// Switch statement

			GameObject moneyCopy = Instantiate(moneyPrefab);
			dialogueText.text = "Make it rain!!";

			Destroy(moneyCopy, 1.8f);



			bool isDead = enemyUnit.TakeDamage(6);
			enemyHUD.SetHP(enemyUnit.currentHP);


			//Wait for move to finish for a bit
			yield return new WaitForSeconds(4f);

			if (isDead)
			{
				state = BattleState.WON;
				EndBattle();
			}
			else
			{
				state = BattleState.ENEMYTURN;
				StartCoroutine(EnemyTurn());
			}
		}
		else
		{
			bool isDead = enemyUnit.TakeDamage(playerUnit.damage);

			if (isDead)
			{
				state = BattleState.WON;
				EndBattle();
			}
			else
			{
				state = BattleState.ENEMYTURN;
				StartCoroutine(EnemyTurn());
			}
		}

	}

	IEnumerator EnemyTurn()
	{
		
		HandleScript.canSpin = false;

		dialogueText.text = enemyUnit.unitName + " attacks!";
		yield return new WaitForSeconds(2f);

		float probability = Random.Range(0f, 1f);
		if(probability > .8)
        {
			//enemyAnimator.SetBool("charms", true);
			GameObject charmCopy = Instantiate(charmPrefab);
			dialogueText.text = "Get Charmed!!!";
			Destroy(charmCopy, 1.5f);
			yield return new WaitForSeconds(2f);
			dialogueText.text = "Your turn is skipped.";
			yield return new WaitForSeconds(2f);

			StartCoroutine(EnemyTurn());
		}
		else 
		{
			bool isDead = playerUnit.TakeDamage(enemyUnit.damage);
			enemyAnimator.SetBool("isAttackingBasic", true);

			yield return new WaitForSeconds(2.3f);
			playerHUD.SetHP(playerUnit.currentHP);
			enemyAnimator.SetBool("isAttackingBasic", false);

			yield return new WaitForSeconds(2.3f);

			if (isDead)
			{
				state = BattleState.LOST;
				EndBattle();
			}
			else
			{
				state = BattleState.PLAYERTURN;
				PlayerTurn();
			}
			
		}



	}

	void EndBattle()
	{
		HandleScript.canSpin = false;

		if (state == BattleState.WON)
		{
			dialogueText.text = "You won the battle!";
		} else if (state == BattleState.LOST)
		{
			dialogueText.text = "You were defeated.";
		}
	}

	void PlayerTurn()
	{
		state = BattleState.PLAYERTURN;

		HandleScript.canSpin = true;
		dialogueText.text = "PULL THE LEVER!";
	}

	IEnumerator PlayerHeal()
	{
		playerUnit.Heal(5);

		playerHUD.SetHP(playerUnit.currentHP);
		dialogueText.text = "You feel renewed strength!";

		yield return new WaitForSeconds(2f);

		state = BattleState.ENEMYTURN;
		StartCoroutine(EnemyTurn());
	}

	public void OnAttackButton()
	{
		if (state != BattleState.PLAYERTURN)
			return;

		StartCoroutine(PlayerAttack());
	}

	public void onSpin()
    {
		// return if not playerTurn
		if (state != BattleState.PLAYERTURN)
			return;

		StartCoroutine(PlayerAttackReel());
		
	}

	public void OnHealButton()
	{
		if (state != BattleState.PLAYERTURN)
			return;

		StartCoroutine(PlayerHeal());
	}

}
