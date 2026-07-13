using UnityEngine;

public static class Support
{
    public static uint GetUCID(int roomID, int charaID, bool is1P)
    {
        int is1PVal = is1P ? 1 : 0; 
        return (uint)((charaID * 1000) + (is1PVal * 100) + roomID);
    }
}