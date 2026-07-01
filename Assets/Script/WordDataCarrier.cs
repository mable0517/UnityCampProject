using UnityEngine;
using System.Collections.Generic;

public class WordDataCarrier : MonoBehaviour
{
    public static WordDataCarrier Instance;
    public List<WordType> selectedWords = new List<WordType>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 변경 시 파괴 차단
        }
        else
        {
            Destroy(gameObject);
        }
    }
}