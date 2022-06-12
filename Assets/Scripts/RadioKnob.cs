using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioKnob : MonoBehaviour
{
    public int Station = 0;
    public Sounds GoalStation = Sounds.RadioStation_Goal;
    public Sounds StationChanging = Sounds.RadioStation_Changing;
    public Sounds[] StationsAvailable = { Sounds.RadioStation_Other2, Sounds.RadioStation_Other1, Sounds.RadioStation_Goal, Sounds.RadioStation_Other3 };
    public LevelGoal LevelGoal;
    public Transform TuningKnob;

    public bool PowerIsOn = false;

    public void TurnOff()
    {
        PowerIsOn = false;
        StopRadio();
    }

    public void TurnOn()
    {
        PowerIsOn = true;
        AudioController.Current.PlayOnRadio(StationsAvailable[Station]);
        LevelGoal.SetCompletionStatus(StationsAvailable[Station] == GoalStation);
    }

    public void ChangeStation()
    {
        if (!PowerIsOn)
            return;

        if ( TuningKnob != null )
            TuningKnob.transform.Rotate(Vector3.forward, 360/StationsAvailable.Length);

        StopAllCoroutines();
        StartCoroutine("ChangeStationCoRoutine");
    }

    public void StopRadio()
    {
        StopAllCoroutines();
        AudioController.Current.StopPlayingRadio();
        LevelGoal.SetInComplete();
    }

    private IEnumerator ChangeStationCoRoutine()
    {
        AudioController.Current.StopPlayingRadio();

        Station = (Station + 1) % StationsAvailable.Length;
        LevelGoal.SetCompletionStatus(StationsAvailable[Station] == GoalStation);

        AudioController.Current.PlayOnRadio(StationChanging);
        yield return new WaitForSeconds(2.5f);

        AudioController.Current.PlayOnRadio(StationsAvailable[Station]);
    }
}

