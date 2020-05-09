using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Intro : MonoBehaviour
{
    private Animation anim;
    private Text introText;
    void Start()
    {
        anim = GetComponent<Animation>();
        introText = GetComponent<Text>();
        StartCoroutine(playIntro());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator playIntro()
    {
        anim.Play("3");
        yield return new WaitForSeconds(1f);
        introText.text = "2";
        anim.Play("2");
        yield return new WaitForSeconds(1f);
        introText.text = "1";
        anim.Play("1");
        yield return new WaitForSeconds(1f);
        introText.text = "Go!";
        anim.Play("Go");
        Debug.Log("Ding!");
    }
}
