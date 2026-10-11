using UnityEngine;

public class GameInfo : MonoBehaviour
{
    public int goal;

    public int getSegments()
    {
        return Segment.AliveCount;
    }

    public int getGoal()
    {
        return goal;
    }
}
