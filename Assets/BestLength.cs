using UnityEngine;

/// <summary>
/// The longest Peggy has ever grown, kept between sessions
/// </summary>
public static class BestLength
{
  private const string Key = "BestLength";

  public static int Value => PlayerPrefs.GetInt(Key, 0);

  public static void Submit(int length)
  {
    if (length <= Value)
    {
      return;
    }
    PlayerPrefs.SetInt(Key, length);
    // WebGL only writes PlayerPrefs to storage on Save
    PlayerPrefs.Save();
  }
}
