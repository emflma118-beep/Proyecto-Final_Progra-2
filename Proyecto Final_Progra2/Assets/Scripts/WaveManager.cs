using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private WaveConfig[] waveConfigs;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    public static event Action<GameState> OnGameStateChanged;
    public static event Action<int, int> OnWaveChanged;
    public static event Action<int> OnEnemiesRemainingChanged;

    private GameState estadoActualJuego = GameState.Preparacion;
    private int indiceOleadaActual = 0;
    private int enemigosRestantes = 0;
    private Coroutine waveCoroutine;
    private Coroutine spawnCoroutine;
    private bool oleadaEnProgreso = false;
    private bool spawnEnProgreso = false;

    public GameState CurrentGameState => estadoActualJuego;
    public int CurrentWaveIndex => indiceOleadaActual;
    public int oleadasTotales => waveConfigs.Length;
    public int EnemiesRemaining => enemigosRestantes;

    private List<GameObject> enemigosActivos = new List<GameObject>();

    void Start()
    {
        SetGameState(GameState.Preparacion);
    }

    public void InicioSistemaOleadas()
    {
        DetenerTodo();
        oleadaEnProgreso = true;
        indiceOleadaActual = 0;
        waveCoroutine = StartCoroutine(ProcesarOleada(indiceOleadaActual));
    }

    private IEnumerator ProcesarOleada(int indiceOleada)
    {
        if (indiceOleada >= waveConfigs.Length) yield break;

        WaveConfig oleadaActual = waveConfigs[indiceOleada];
        OnWaveChanged?.Invoke(indiceOleada + 1, waveConfigs.Length);

        if (debugMode)
            Debug.Log($"Iniciando Oleada {indiceOleada + 1}: {oleadaActual.nombreOleada}");

        SetGameState(GameState.Spawn);
        spawnEnProgreso = true;
        spawnCoroutine = StartCoroutine(SpawnWaveCoroutine(oleadaActual));

        yield return new WaitUntil(() => !spawnEnProgreso);

        SetGameState(GameState.Espera);
        yield return new WaitUntil(() => enemigosRestantes <= 0 || !oleadaEnProgreso);

        if (!oleadaEnProgreso) yield break;

        SetGameState(GameState.OleadaCompletada);

        switch (oleadaActual.oleadaCompleta)
        {
            case OleadaCompletada.Continuar:
                yield return new WaitForSeconds(oleadaActual.tiempoAntesDeOleada);
                if (indiceOleadaActual < waveConfigs.Length - 1)
                {
                    indiceOleadaActual++;
                    waveCoroutine = StartCoroutine(ProcesarOleada(indiceOleadaActual));
                }
                else
                {
                    FinalizarJuego();
                }
                break;

            case OleadaCompletada.EsperarJugador:
                yield return new WaitUntil(() => !oleadaEnProgreso);
                break;

            case OleadaCompletada.Evento:
                yield return new WaitForSeconds(oleadaActual.tiempoAntesDeOleada);
                if (indiceOleadaActual < waveConfigs.Length - 1)
                {
                    indiceOleadaActual++;
                    waveCoroutine = StartCoroutine(ProcesarOleada(indiceOleadaActual));
                }
                else
                {
                    FinalizarJuego();
                }
                break;
        }
    }

    private IEnumerator SpawnWaveCoroutine(WaveConfig wave)
    {
        enemigosRestantes = 0;

        foreach (EnemigoOleada enemigoOleada in wave.enemigo)
        {
            enemigosRestantes += enemigoOleada.cantidadEnemy;
        }

        OnEnemiesRemainingChanged?.Invoke(enemigosRestantes);

        foreach (EnemigoOleada enemigoOleada in wave.enemigo)
        {
            yield return StartCoroutine(SpawnGrupoEnemigosCoroutine(enemigoOleada, wave.tiempoEntreSpawns));
        }

        spawnEnProgreso = false;
    }

    private IEnumerator SpawnGrupoEnemigosCoroutine(EnemigoOleada enemigoOleada, float tiempoEntreSpawns)
    {
        switch (enemigoOleada.patronA)
        {
            case PatronAparicion.Secuencial:
                for (int i = 0; i < enemigoOleada.cantidadEnemy; i++)
                {
                    if (!oleadaEnProgreso) yield break;
                    SpawnEnemigo(enemigoOleada.perfilEnemigo);
                    if (i < enemigoOleada.cantidadEnemy - 1)
                        yield return new WaitForSeconds(tiempoEntreSpawns);
                }
                break;

            case PatronAparicion.Simultaneo:
                for (int i = 0; i < enemigoOleada.cantidadEnemy; i++)
                {
                    SpawnEnemigo(enemigoOleada.perfilEnemigo);
                }
                break;

            case PatronAparicion.Rafaga:
                int enemigosPorRafaga = 3;
                int numeroRafagas = Mathf.CeilToInt((float)enemigoOleada.cantidadEnemy / enemigosPorRafaga);

                for (int rafaga = 0; rafaga < numeroRafagas; rafaga++)
                {
                    if (!oleadaEnProgreso) yield break;

                    int inicio = rafaga * enemigosPorRafaga;
                    int fin = Mathf.Min(inicio + enemigosPorRafaga, enemigoOleada.cantidadEnemy);

                    for (int i = inicio; i < fin; i++)
                    {
                        SpawnEnemigo(enemigoOleada.perfilEnemigo);
                    }

                    if (rafaga < numeroRafagas - 1)
                        yield return new WaitForSeconds(tiempoEntreSpawns * 2);
                }
                break;
        }
    }

    private void SpawnEnemigo(PerfilEnemigo perfil)
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogError("No hay puntos de spawn configurados!");
            return;
        }

        Transform spawnPoint = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
        GameObject enemigo = Instantiate(perfil.prefab, spawnPoint.position, spawnPoint.rotation);

        BaseEnemigo baseEnemigo = enemigo.GetComponent<BaseEnemigo>();
        if (baseEnemigo != null)
        {
            baseEnemigo.Iniciar(perfil, this);
        }

        enemigosActivos.Add(enemigo);
    }

    public void EnemigoDerrotado(GameObject enemigo)
    {
        if (enemigosActivos.Contains(enemigo))
        {
            enemigosActivos.Remove(enemigo);
            enemigosRestantes--;
            OnEnemiesRemainingChanged?.Invoke(enemigosRestantes);
        }
    }

    public void SiguienteOleada()
    {
        if (!oleadaEnProgreso) return;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        spawnEnProgreso = false;

        LimpiarEnemigosActuales();

        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
            waveCoroutine = null;
        }

        if (indiceOleadaActual < waveConfigs.Length - 1)
        {
            indiceOleadaActual++;
            waveCoroutine = StartCoroutine(ProcesarOleada(indiceOleadaActual));
        }
        else
        {
            FinalizarJuego();
        }
    }

    public void TerminarOleadaActual()
    {
        if (!oleadaEnProgreso) return;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        spawnEnProgreso = false;

        LimpiarEnemigosActuales();

        SetGameState(GameState.OleadaCompletada);
    }

    private void FinalizarJuego()
    {
        SetGameState(GameState.Finalizar);
        oleadaEnProgreso = false;

        if (debugMode)
            Debug.Log("¡Todas las oleadas completadas!");
    }

    public void SaltarAOleada(int oleadaIndex)
    {
        if (oleadaIndex < 0 || oleadaIndex >= waveConfigs.Length) return;

        DetenerTodo();

        LimpiarEnemigosActuales();

        indiceOleadaActual = oleadaIndex;
        oleadaEnProgreso = true;

        waveCoroutine = StartCoroutine(ProcesarOleada(indiceOleadaActual));
    }

    private void DetenerTodo()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
            waveCoroutine = null;
        }

        spawnEnProgreso = false;
    }

    private void LimpiarEnemigosActuales()
    {
        foreach (GameObject enemigo in enemigosActivos)
        {
            if (enemigo != null)
            {
                BaseEnemigo baseEnemigo = enemigo.GetComponent<BaseEnemigo>();
                if (baseEnemigo != null)
                {
                    baseEnemigo.DestruirDirectamente();
                }
                else
                {
                    Destroy(enemigo);
                }
            }
        }
        enemigosActivos.Clear();
        enemigosRestantes = 0;
        OnEnemiesRemainingChanged?.Invoke(enemigosRestantes);
    }

    private void SetGameState(GameState newState)
    {
        estadoActualJuego = newState;
        OnGameStateChanged?.Invoke(newState);

        if (debugMode)
            Debug.Log($"Estado del Juego: {newState}");
    }

    public void StopAllWaves()
    {
        DetenerTodo();
        LimpiarEnemigosActuales();
        oleadaEnProgreso = false;
        SetGameState(GameState.Preparacion);
    }
}

public enum GameState
{
    Preparacion,
    Spawn,
    Espera,
    OleadaCompletada,
    Finalizar
}
