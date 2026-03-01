using System;
using UnityEngine;

[System.Serializable]
public class PaqueteDato
{
    public string id;
    public int tamañoCarga;
    public float tiempoLlegada;

    public PaqueteDato(int tamañoCarga, float tiempoLlegada)
    {
        id = Guid.NewGuid().ToString();
        this.tamañoCarga = tamañoCarga;
        this.tiempoLlegada = tiempoLlegada;
    }
}
