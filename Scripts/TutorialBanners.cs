using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialBanners : MonoBehaviour
{
    [SerializeField] private GameObject tutorialWindow;
    [SerializeField] private TextMeshProUGUI tutorialText;

    string moveLeftRight = "An unmoving Shinobi/Kunoichi is useless\n\nPress A or Left Arrow to move left\nPress D or Right Arrow to move right";
    string jump = "Staying on the ground accomplishes nothing\n\nPress the Spacebar key to jump";
    string pauseAndReload = "A Shinobi/Kunoichi has limited time control and great memory\n\nPress the Esc or P Key to pause\nClick Restart to reload the last checkpoint";
    string wallPush = "A Shinobi/Kunoichi has strong legs\n\nJump again on a wall to push yourself vertically and overcome high obstacles";
    string wallJump = "A Shinobi/Kunoichi disregards gravity\n\nJump again after a wall push to push yourself to unreachable heights";
    string crouchAndSlide = "A Shinobi/Kunoichi plays with sizes\n\nPress any Ctrl or the C Key to crouch on the spot\nCrouch while moving to slide under obstacles";
    string useLadder = "A Shinobi/Kunoichi has strong arms\n\nPress W or Up Arrow to move up a ladder\nPress S or Down Arrow to move down a ladder";
    string checkpoint = "A Shinobi/Kunoichi remembers boxes, boxes are important\n\nCrossing boxes activates a checkpoint\nYou will respawn there after dying";
    string avoidHazards = "A Shinobi/Kunoichi avoids what is deadly\n\nAvoid water at all costs\nPress the V key or Numpad 0 to safely walk across bamboo spikes";
    string attack = "A Shinobi/Kunoichi is deadly\n\nPress the H or any Shift Key to attack\nPress Left Alt or the J key to throw shurikens";
    string hide = "A Shinobi/Kunoichi knows to be invisible\n\nCrouch in front of stones, bushes, trees, etc. to hide from the enemies\nUnaware enemies die quick";
    string parry = "A Shinobi/Kunoichi has lightning reflexes\n\nPress the Return, Tab or K Key to deflect incoming enemy projectiles";
    string pickUps = "A Shinobi/Kunoichi has an eye for gold\n\nPick coins to heal your wounds\nTen coins grant a new life";
    string mission = "A Shinobi/Kunoichi lives to serve\n\nGet to Hideki Castle as fast as possible\nWe must retrieve their war plans\nRival clans seek them, time flies";

    private void Start()
    {
        this.tutorialWindow = GameObject.Find("Tutorial Popup");
        this.tutorialText = GameObject.Find("Tutorial Text").GetComponent<TextMeshProUGUI>();

        this.tutorialWindow.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        this.tutorialWindow.SetActive(true);
        this.SetTutorialText(collision.transform.position.x);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        this.tutorialWindow.SetActive(false);
    }

    private void SetTutorialText(float posX)
    {
        switch (posX)
        {
            case float x when x > 51f & x < 54f:
                this.tutorialText.text = this.moveLeftRight;
                break;

            case float x when x > 58f & x < 61f:
                this.tutorialText.text = this.jump;
                break;

            case float x when x > 70f & x < 73f:
                this.tutorialText.text = this.pauseAndReload;
                break;

            case float x when x > 81f & x < 84f:
                this.tutorialText.text = this.wallPush;
                break;

            case float x when x > 93f & x < 96f:
                this.tutorialText.text = this.wallJump;
                break;

            case float x when x > 110f & x < 113f:
                this.tutorialText.text = this.crouchAndSlide;
                break;

            case float x when x > 133f & x < 136f:
                this.tutorialText.text = this.useLadder;
                break;

            case float x when x > 140f & x < 143f:
                this.tutorialText.text = this.checkpoint;
                break;

            case float x when x > 149f & x < 152f:
                this.tutorialText.text = this.avoidHazards;
                break;

            case float x when x > 165f & x < 168f:
                this.tutorialText.text = this.attack;
                break;

            case float x when x > 170f & x < 173f:
                this.tutorialText.text = this.hide;
                break;

            case float x when x > 174f & x < 177f:
                this.tutorialText.text = this.parry;
                break;

            case float x when x > 200f & x < 203f:
                this.tutorialText.text = this.pickUps;
                break;

            case float x when x > 208f & x < 211f:
                this.tutorialText.text = this.mission;
                break;
        }
    }

    private void OnDestroy()
    {
        if(this.tutorialWindow != null)
            this.tutorialWindow.SetActive(true);
    }
}
