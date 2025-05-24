using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class SceneChanger : MonoBehaviour
{

    [SerializeField] private string nombreEscena;


    public void CambiarDeEscena()
    {
        SceneManager.LoadScene(nombreEscena);
    }
}
