using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Intro : MonoBehaviour
{
    private Animation anim;
    private Text introText;

    public GameObject three;
    public GameObject two;
    public GameObject one;
    public GameObject go;

    public List<AudioClip> audio;

    private SoundEffectManager SoundMan;

    void Start()
    {

        introText = GetComponent<Text>();
        StartCoroutine(playIntro());

        SoundMan = GameObject.FindGameObjectWithTag("SoundEffectManager").GetComponent<SoundEffectManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator playIntro()
    {
        GameObject.FindGameObjectWithTag("Head").GetComponent<Movement>().enabled = false;
        GameObject.Find("MainMusic").GetComponent<AudioSource>().enabled = false;

        yield return new WaitForSeconds(0.3f);

        three.transform.GetChild(0).GetComponent<Text>().enabled = true;
        three.GetComponent<Animator>().Play("3");

        yield return new WaitForSeconds(0.5f);
        GetComponent<AudioSource>().clip = audio[0];
        GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(0.5f);
        three.transform.GetChild(0).GetComponent<Text>().enabled = false;

        two.transform.GetChild(0).GetComponent<Text>().enabled = true;
        two.GetComponent<Animator>().Play("2");

        yield return new WaitForSeconds(0.5f);
        GetComponent<AudioSource>().clip = audio[1];
        GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(0.5f);
        two.transform.GetChild(0).GetComponent<Text>().enabled = false;
       
        one.transform.GetChild(0).GetComponent<Text>().enabled = true;
        one.GetComponent<Animator>().Play("1");

        yield return new WaitForSeconds(0.5f);
        GetComponent<AudioSource>().clip = audio[2];
        GetComponent<AudioSource>().Play();
        yield return new WaitForSeconds(0.5f);
        one.transform.GetChild(0).GetComponent<Text>().enabled = false;

        go.transform.GetChild(0).GetComponent<Text>().enabled = true;
        go.GetComponent<Animator>().Play("Go");

        yield return new WaitForSeconds(0.3f);
        GetComponent<AudioSource>().clip = audio[3];
        GetComponent<AudioSource>().Play();

        GameObject.FindGameObjectWithTag("Head").GetComponent<Movement>().enabled = true;
        yield return new WaitForSeconds(0.7f);
        GameObject.Find("MainMusic").GetComponent<AudioSource>().enabled = true;
        SoundMan.playBork();
        go.transform.GetChild(0).GetComponent<Text>().enabled = false;
        Debug.Log("Ding!");
    }
}
