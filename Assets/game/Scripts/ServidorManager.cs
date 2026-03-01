using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServidorManager : MonoBehaviour
{
    [Header("Estructuras principales (obligatorias)")]
    public Queue<PaqueteDato> colaProcesamiento = new Queue<PaqueteDato>();
    public Dictionary<string, PaqueteDato> historialProcesados = new Dictionary<string, PaqueteDato>();

    [Header("Parámetros de simulación")]
    [Tooltip("Segundos entre llegadas automáticas (sugerido 2–4).")]
    [Range(0.5f, 10f)] public float intervaloLlegada = 3f;
    public Vector2Int rangoTamañoCarga = new Vector2Int(10, 200);

    [Header("Métricas (obligatorio)")]
    [SerializeField] private float tiempoTotalEspera = 0f;
    [SerializeField] private int totalProcesados = 0;

    private UIManager ui;
    private bool procesando = false;

    private void Awake()
    {
        ui = FindFirstObjectByType<UIManager>();
    }

    private void Start()
    {
        StartCoroutine(CorrutinaLlegadas());
        ActualizarUI();
    }

    private IEnumerator CorrutinaLlegadas()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloLlegada);

            int cuantos = Random.Range(1, 5);
            for (int i = 0; i < cuantos; i++)
            {
                int carga = Random.Range(rangoTamañoCarga.x, rangoTamañoCarga.y + 1);
                var p = new PaqueteDato(carga, Time.time);
                colaProcesamiento.Enqueue(p);
            }

            ActualizarUI();
        }
    }

    public void ProcesarSiguiente()
    {
        if (procesando) return;

        if (colaProcesamiento.Count == 0)
        {
            if (ui != null) ui.MostrarSinPaquetes();
            return;
        }

        StartCoroutine(ProcesarUnPaquete());
    }

    private IEnumerator ProcesarUnPaquete()
    {
        procesando = true;

        PaqueteDato paquete = colaProcesamiento.Dequeue();

        if (historialProcesados.ContainsKey(paquete.id))
        {
            if (ui != null) ui.MostrarIdDuplicado(paquete.id);
            ActualizarUI();
            procesando = false;
            yield break;
        }

        historialProcesados.Add(paquete.id, paquete);
        float espera = Time.time - paquete.tiempoLlegada;
        tiempoTotalEspera += espera;
        totalProcesados++;

        if (ui != null) ui.SetUltimoProcesado(paquete, espera);
        ActualizarUI();

        yield return new WaitForSeconds(0.1f);
        procesando = false;
    }

    public float GetPromedioEspera()
    {
        if (totalProcesados <= 0) return 0f;
        return tiempoTotalEspera / totalProcesados;
    }

    public int GetColaCount() => colaProcesamiento.Count;
    public int GetHistorialCount() => historialProcesados.Count;

    public bool TryBuscarPorId(string id, out PaqueteDato paquete)
    {
        return historialProcesados.TryGetValue(id, out paquete);
    }

    private void ActualizarUI()
    {
        if (ui == null) return;
        ui.RefrescarContadores(GetColaCount(), GetHistorialCount(), GetPromedioEspera());
        ui.RefrescarEstadoSaturacion(GetColaCount());
    }
}