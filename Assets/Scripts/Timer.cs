using UnityEngine;

using TMPro;
public class Timer: MonoBehaviour
{
    private float timePassed = 0;

    public TextMeshProUGUI timerDisplay;

    private void Update()
    {
        timePassed += Time.deltaTime;

        int seconds = Mathf.FloorToInt(timePassed) % 60;

        int minutes = Mathf.FloorToInt(timePassed / 60);

        int miliseconds = Mathf.FloorToInt(timePassed - seconds) * 100;

       // timerDisplay.text = timePassed.ToString();

        timerDisplay.text = seconds.ToString("00") +":" + minutes.ToString("00") + ":" + miliseconds.ToString("80");

        //Debug.Log(timePassed);

    }

}
