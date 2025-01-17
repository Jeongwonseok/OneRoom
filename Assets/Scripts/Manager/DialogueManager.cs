﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{

    [SerializeField] GameObject go_DialogueBar;
    [SerializeField] GameObject go_DialogueNameBar;

    [SerializeField] Text txt_Dialogue;
    [SerializeField] Text txt_Name;

    Dialogue[] dialogues;

    bool isDialogue = false;  // 대화중일때 true
    bool isNext = false;      // 특정 키 입력 대기

    [Header("텍스트 출력 딜레이")]
    [SerializeField] float textDelay;

    int lineCount = 0;  // 대화 카운트
    int contextCount = 0; // 대사 카운트

    InteracrionController theIC;
    CameraController theCam;
    SplashManager theSplashManager;
    SpriteManager theSpriteManager;
    CutSceneManager theCutSceneManager;
    SlideManager theSlideManager;

    void Start()
    {
        theIC = FindObjectOfType<InteracrionController>();
        theCam = FindObjectOfType<CameraController>();
        theSpriteManager = FindObjectOfType<SpriteManager>();
        theSplashManager = FindObjectOfType<SplashManager>();
        theCutSceneManager = FindObjectOfType<CutSceneManager>();
        theSlideManager = FindObjectOfType<SlideManager>();
    }

    void Update()
    {
        if(isDialogue)
        {
            if(isNext)
            {
                if(Input.GetKeyDown(KeyCode.Space))
                {
                    isNext = false;
                    txt_Dialogue.text = "";
                    if (++contextCount < dialogues[lineCount].contexts.Length)
                    {
                        StartCoroutine(TypeWriter());
                    }
                    else
                    {
                        contextCount = 0;
                        if(++lineCount < dialogues.Length)
                        {
                            StartCoroutine(CameraTargettingType());
                        }
                        else
                        {
                            StartCoroutine(EndDialogue());

                        }
                    }
                }
            }
        }
    }

    public void ShowDialogue(Dialogue[] p_dialogues)
    {
        isDialogue = true;
        txt_Dialogue.text = "";
        txt_Name.text = "";
        theIC.SettingUI(false);

        dialogues = p_dialogues;
        theCam.CamOriginSetting();

        StartCoroutine(CameraTargettingType());

    }

    IEnumerator CameraTargettingType()
    {
        switch(dialogues[lineCount].cameraType)
        {
            case CameraType.FadeIn: SettingUI(false); SplashManager.isFinished = false; StartCoroutine(theSplashManager.FadeIn(false, true)); yield return new WaitUntil(() => SplashManager.isFinished); break;
            case CameraType.FadeOut: SettingUI(false); SplashManager.isFinished = false; StartCoroutine(theSplashManager.FadeOut(false, true)); yield return new WaitUntil(() => SplashManager.isFinished); break;
            case CameraType.FlashIn: SettingUI(false); SplashManager.isFinished = false; StartCoroutine(theSplashManager.FadeIn(true, true)); yield return new WaitUntil(() => SplashManager.isFinished); break;
            case CameraType.FlashOut: SettingUI(false); SplashManager.isFinished = false; StartCoroutine(theSplashManager.FadeOut(true, true)); yield return new WaitUntil(() => SplashManager.isFinished); break;
            case CameraType.ObjectFront: theCam.CameraTargetting(dialogues[lineCount].tf_Target); break;
            case CameraType.Reset: theCam.CameraTargetting(null, 0.05f, true, false); break;
            case CameraType.ShowCutScene: SettingUI(false); CutSceneManager.isFinished = false; StartCoroutine(theCutSceneManager.CutSceneCoroutine(dialogues[lineCount].spriteName[contextCount], true)); yield return new WaitUntil(() => CutSceneManager.isFinished); break;
            case CameraType.HideCutScene: SettingUI(false); CutSceneManager.isFinished = false; StartCoroutine(theCutSceneManager.CutSceneCoroutine(null, false)); yield return new WaitUntil(() => CutSceneManager.isFinished); theCam.CameraTargetting(dialogues[lineCount].tf_Target); break;
            case CameraType.AppearSlideCG: SlideManager.isFinished = false; StartCoroutine(theSlideManager.AppearSlide(SplitSlideCGName())); yield return new WaitUntil(() => SlideManager.isFinished); theCam.CameraTargetting(dialogues[lineCount].tf_Target); break;
            case CameraType.DisappearSlideCG: SlideManager.isFinished = false; StartCoroutine(theSlideManager.DisappearSlide()); yield return new WaitUntil(() => SlideManager.isFinished); theCam.CameraTargetting(dialogues[lineCount].tf_Target); break;
            case CameraType.ChangeSlideCG: SlideManager.isChanged = false; StartCoroutine(theSlideManager.ChangeSlide(SplitSlideCGName())); yield return new WaitUntil(() => SlideManager.isChanged); theCam.CameraTargetting(dialogues[lineCount].tf_Target); break;
        }
        StartCoroutine(TypeWriter());
    }

    string SplitSlideCGName()
    {
        string t_Text = dialogues[lineCount].spriteName[contextCount];
        string[] t_Array = t_Text.Split(new char[] { '/' });
        if(t_Array.Length <= 1)
        {
            return t_Array[0];
        }
        else
        {
            return t_Array[1];
        }
    }

    IEnumerator EndDialogue()
    {
        SettingUI(false);
        if(theCutSceneManager.CheckCutScene())
        {
            CutSceneManager.isFinished = false;
            StartCoroutine(theCutSceneManager.CutSceneCoroutine(null, false));
            yield return new WaitUntil(() => CutSceneManager.isFinished);
        }
        isDialogue = false;
        contextCount = 0;
        lineCount = 0;
        dialogues = null;
        isNext = false;

        theCam.CameraTargetting(null, 0.05f, true, true);

        SettingUI(false);
    }

    void ChangeSprite()
    {
        if (dialogues[lineCount].tf_Target != null)
        {
            if (dialogues[lineCount].spriteName[contextCount] != "")
            {
                StartCoroutine(theSpriteManager.SpriteChangeCoroutine(
                                                dialogues[lineCount].tf_Target,
                                                dialogues[lineCount].spriteName[contextCount].Split(new char[] {'/'})[0]));
            }
        }
    }

    void PlaySound()
    {
        if(dialogues[lineCount].voiceName[contextCount] != "")
        {
            SoundManager.instance.PlaySound(dialogues[lineCount].voiceName[contextCount], 2);
        }
    }

    IEnumerator TypeWriter()
    {
        SettingUI(true);
        ChangeSprite();
        PlaySound();

        string t_ReplaceText = dialogues[lineCount].contexts[contextCount];
        t_ReplaceText = t_ReplaceText.Replace("'", ",");
        t_ReplaceText = t_ReplaceText.Replace("\\n", "\n");


        bool t_white = false, t_yellow = false, t_cyan = false;
        bool t_ignore = false;

        for (int i = 0; i < t_ReplaceText.Length; i++)
        {
            switch(t_ReplaceText[i])
            {
                case 'ⓦ': t_white = true; t_yellow = false; t_cyan = false; t_ignore = true; break;
                case 'ⓨ': t_white = false; t_yellow = true; t_cyan = false; t_ignore = true; break;
                case 'ⓒ': t_white = false; t_yellow = false; t_cyan = true; t_ignore = true; break;
                case '①': StartCoroutine(theSplashManager.Splash()); SoundManager.instance.PlaySound("Emotion1", 1); t_ignore = true; break;
                case '②': StartCoroutine(theSplashManager.Splash()); SoundManager.instance.PlaySound("Emotion2", 1); t_ignore = true; break;
            }

            string t_letter = t_ReplaceText[i].ToString();

            if(!t_ignore)
            {
                if(t_white)
                    t_letter = "<color=#ffffff>" + t_letter + "</color>";
                else if (t_yellow)
                    t_letter = "<color=#ffff00>" + t_letter + "</color>";
                else if (t_cyan)
                    t_letter = "<color=#42DEE3>" + t_letter + "</color>";
                txt_Dialogue.text += t_letter;
            }
            t_ignore = false;


            yield return new WaitForSeconds(textDelay);
        }

        isNext = true;

    }

    void SettingUI(bool p_flag)
    {
        go_DialogueBar.SetActive(p_flag);

        if(p_flag)
        {
            if(dialogues[lineCount].name == "")
            {
                go_DialogueNameBar.SetActive(false);
            }
            else
            {
                go_DialogueNameBar.SetActive(true);
                txt_Name.text = dialogues[lineCount].name;
            }
        }
        else
        {
            go_DialogueNameBar.SetActive(false);
        }

    }
}
