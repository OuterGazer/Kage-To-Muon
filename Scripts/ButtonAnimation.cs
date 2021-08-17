using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonAnimation : MonoBehaviour
{
    private Vector3 originalSize = new Vector3(0.6f, 0.6f, 0.6f);
    private Vector3 clickedSize = new Vector3(0.55f, 0.55f, 0.55f);

    private Image buttonImage;
    private TextMeshProUGUI buttonText;

    [SerializeField] TextMeshProUGUI helpText;

    private void Awake()
    {
        this.buttonImage = this.gameObject.GetComponent<Image>();
        this.buttonText = this.gameObject.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        this.buttonImage.color = Color.white;
        this.buttonText.color = Color.white;

        if(this.helpText != null)
            this.helpText.text = "";
    }

    public void OnPointerDown()
    {
        this.gameObject.transform.localScale = this.clickedSize;
    }

    public void OnPointerUp()
    {
        this.gameObject.transform.localScale = this.originalSize;
    }

    public void OnPointerEnter()
    {
        this.buttonImage.color = Color.grey;
        this.buttonText.color = Color.grey;

        this.SetHelpText(this.gameObject.name);
    }

    public void OnPointerExit()
    {
        this.buttonImage.color = Color.white;
        this.buttonText.color = Color.white;

        this.ClearHelpText();
    }

    private void SetHelpText(string name)
    {
        switch (name)
        {
            case "Start Button":
                this.helpText.text = "Start game";
                break;

            case "Resume Button":
                this.helpText.text = "Resume game";
                break;

            case "Restart Button":
                this.helpText.text = "Restart from\nlast checkpoint";
                break;

            case "Options Button":
                this.helpText.text = "Set music/SFX volume";
                break;

            case "Credits Button":
                this.helpText.text = "See the credits\nfor the game";
                break;

            case "Exit Button":
                this.helpText.text = "Exit to main menu";
                break;

            case "Quit Button":
                this.helpText.text = "Quit to desktop";
                break;

            case "Ninja Button":
                this.helpText.text = "A moving shadow. Has greater endurance\nalbeit slightly slower. His shurikens\nreach further, sacrificing impact strength.";
                break;

            case "Kunoichi Button":
                this.helpText.text = "A silent breeze. Moves quicker but\nendure less hits. Her shurikens are deadlier,\nhaving a shorter reach";
                break;

            default:
                //this.helpText.text = "";
                break;
        }
    }

    private void ClearHelpText()
    {
        if(this.helpText != null)
            this.helpText.text = "";
    }
}
