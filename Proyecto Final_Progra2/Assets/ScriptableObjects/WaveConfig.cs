using UnityEngine;
using System;

[CreateAssetMenu(fileName = "WaveConfig", menuName = "Wave System/Wave Config")]
public class WaveConfig : ScriptableObject
{
    [Header("Configuracion General")]
    public string nombreOleada;
    public int numeroOleada;

    [Header("Enemigos")]
    public EnemigoOleada[] enemigo;

    [Header("Tiempo")]
    public float tiempoEntreSpawns = 1f;
    public float duracionOleada = 30f;
    public float tiempoAntesDeOleada = 5f;

    [Header("Configuracion Extra")]
    public bool jefeDeOleada = false;
    public OleadaCompletada oleadaCompleta = OleadaCompletada.Continuar;
}

[Serializable]
public struct EnemigoOleada
{
    public PerfilEnemigo perfilEnemigo;
    public int cantidadEnemy;
    public PatronAparicion patronA;
}

public enum PatronAparicion
{
    Secuencial,
    Simultaneo,
    Rafaga
}

public enum OleadaCompletada
{
    Continuar,
    EsperarJugador,
    Evento
}