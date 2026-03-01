using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Textos principales")]
    [SerializeField] private TMP_Text ColaText;
    [SerializeField] private TMP_Text historialText;
    [SerializeField] private TMP_Text promedioText;
    [SerializeField] private TMP_Text ultimoProcesadoText;
    [SerializeField] private TMP_Text estadoServidorText;

    [Header("Buscador por ID")]
    [SerializeField] private TMP_InputField buscarInput;
    [SerializeField] private TMP_Text buscarResultadoText;
    [SerializeField] private Button buscarButton;

    [Header("Botón Procesar Siguiente")]
    [SerializeField] private Button procesarButton;

    [Header("Configuración de saturación")]
    [Tooltip("Cantidad de paquetes en cola que activa el estado SATURADO (enunciado: 20).")]
    [SerializeField] private int umbralSaturacion = 20;
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorSaturado = Color.red;

    private ServidorManager servidor;

    private void Awake()
    {
        AutoAsignarReferencias();
        servidor = FindFirstObjectByType<ServidorManager>();

        if (procesarButton != null)
            procesarButton.onClick.AddListener(OnProcesarClicked);

        if (buscarButton != null)
            buscarButton.onClick.AddListener(BuscarPorId);
    }

    private void Start()
    {
        if (ultimoProcesadoText != null)
            ultimoProcesadoText.text = "Sin paquetes procesados aún.";
        if (buscarResultadoText != null)
            buscarResultadoText.text = "";
        RefrescarEstadoSaturacion(0);
    }

    private void AutoAsignarReferencias()
    {
        TMP_Text[] textos = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Button[] botones = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        TMP_InputField[] inputs = FindObjectsByType<TMP_InputField>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var t in textos)
        {
            if (t.gameObject.name == "ColaText") ColaText = t;
            if (t.gameObject.name == "HistorialText") historialText = t;
            if (t.gameObject.name == "PromedioText") promedioText = t;
            if (t.gameObject.name == "UltimoProcesadoText") ultimoProcesadoText = t;
            if (t.gameObject.name == "EstadoServidorText" ||
                t.gameObject.name == "Estado1ServidorText") estadoServidorText = t;
            if (t.gameObject.name == "BuscarResultadoText") buscarResultadoText = t;
        }

        foreach (var b in botones)
        {
            if (b.gameObject.name == "ProcesarSiguienteButton") procesarButton = b;
            if (b.gameObject.name == "BuscarButton") buscarButton = b;
        }

        foreach (var i in inputs)
        {
            if (i.gameObject.name == "InputField (TMP)") buscarInput = i;
        }
    }

    public void RefrescarContadores(int colaCount, int historialCount, float promedioEspera)
    {
        if (ColaText != null) ColaText.text = $"En cola: {colaCount}";
        if (historialText != null) historialText.text = $"Total procesados: {historialCount}";
        if (promedioText != null) promedioText.text = $"Promedio espera: {promedioEspera:0.00} s";
    }

    public void RefrescarEstadoSaturacion(int colaCount)
    {
        if (estadoServidorText == null) return;

        bool saturado = colaCount > umbralSaturacion;
        estadoServidorText.text = saturado ? "⚠ SERVIDOR SATURADO" : "Servidor OK";
        estadoServidorText.color = saturado ? colorSaturado : colorNormal;
    }

    public void SetUltimoProcesado(PaqueteDato p, float espera)
    {
        if (ultimoProcesadoText == null) return;
        ultimoProcesadoText.text =
            $"Último procesado:\n" +
            $"ID: {p.id}\n" +
            $"Tamaño: {p.tamañoCarga} KB\n" +
            $"Espera: {espera:0.00} s";
    }

    public void MostrarSinPaquetes()
    {
        if (ultimoProcesadoText != null)
            ultimoProcesadoText.text = "No hay paquetes en cola para procesar.";
    }

    public void MostrarIdDuplicado(string id)
    {
        if (ultimoProcesadoText != null)
            ultimoProcesadoText.text = $"⚠ ID duplicado detectado ({id}).";
    }

    private void OnProcesarClicked()
    {
        if (servidor != null)
            servidor.ProcesarSiguiente();
    }

    public void BuscarPorId()
    {
        if (buscarResultadoText == null || buscarInput == null || servidor == null) return;

        string id = buscarInput.text.Trim();

        if (string.IsNullOrWhiteSpace(id))
        {
            buscarResultadoText.text = "Ingrese un ID válido.";
            return;
        }

        if (servidor.TryBuscarPorId(id, out PaqueteDato p))
            buscarResultadoText.text = $"✔ Encontrado.\nTamaño: {p.tamañoCarga} KB\nLlegó en: {p.tiempoLlegada:0.00} s";
        else
            buscarResultadoText.text = "✘ No existe ese ID en el historial.";
    }
}