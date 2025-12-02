using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaveManager))]
public class WaveManagerEditor : Editor
{
    private SerializedProperty waveConfigs;
    private SerializedProperty spawnPoints;
    private SerializedProperty debugMode;

    private WaveManager waveManager;
    private bool mostrarControlesOleada = true;
    private bool mostrarSpawnPoints = true;
    private bool mostrarDebug = true;

    private void OnEnable()
    {
        waveConfigs = serializedObject.FindProperty("waveConfigs");
        spawnPoints = serializedObject.FindProperty("spawnPoints");
        debugMode = serializedObject.FindProperty("debugMode");

        waveManager = (WaveManager)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Sistema de Oleadas", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Ahora solo una oleada se ejecuta a la vez", MessageType.Info);
        EditorGUILayout.Space();

        mostrarControlesOleada = EditorGUILayout.Foldout(mostrarControlesOleada, "Configuración de Oleadas", true);
        if (mostrarControlesOleada)
        {
            if (waveConfigs != null)
            {
                EditorGUILayout.PropertyField(waveConfigs, new GUIContent("Configuraciones"));
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Controles Principales", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Iniciar Sistema", GUILayout.Height(30)))
            {
                waveManager?.InicioSistemaOleadas();
            }
            if (GUILayout.Button("Detener Todo", GUILayout.Height(30)))
            {
                waveManager?.StopAllWaves();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Control de Oleada Actual", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Terminar Oleada", GUILayout.Height(25)))
            {
                waveManager?.TerminarOleadaActual();
            }

            if (GUILayout.Button("Siguiente Oleada", GUILayout.Height(25)))
            {
                waveManager?.SiguienteOleada();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Saltar a Oleada", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("1")) waveManager?.SaltarAOleada(0);
            if (GUILayout.Button("2")) waveManager?.SaltarAOleada(1);
            if (GUILayout.Button("3")) waveManager?.SaltarAOleada(2);
            if (GUILayout.Button("4")) waveManager?.SaltarAOleada(3);
            if (GUILayout.Button("5")) waveManager?.SaltarAOleada(4);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox("Saltar detendrá la oleada actual y comenzará la nueva", MessageType.None);
        }

        mostrarSpawnPoints = EditorGUILayout.Foldout(mostrarSpawnPoints, "Puntos de Aparición", true);
        if (mostrarSpawnPoints)
        {
            if (spawnPoints != null)
            {
                EditorGUILayout.PropertyField(spawnPoints, new GUIContent("Puntos"));
            }

            if (GUILayout.Button("Buscar Puntos (Tag: SpawnPoint)"))
            {
                BuscarSpawnPoints();
            }
        }

        mostrarDebug = EditorGUILayout.Foldout(mostrarDebug, "Depuración", true);
        if (mostrarDebug)
        {
            if (debugMode != null)
            {
                EditorGUILayout.PropertyField(debugMode, new GUIContent("Modo Debug"));

                if (debugMode.boolValue)
                {
                    EditorGUILayout.HelpBox("Se mostrarán logs en consola", MessageType.Info);
                }
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Estado Actual", EditorStyles.boldLabel);

        if (waveManager != null)
        {
            EditorGUILayout.LabelField($"Estado: {waveManager.CurrentGameState}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Oleada Actual: {waveManager.CurrentWaveIndex + 1} / {waveManager.oleadasTotales}");
            EditorGUILayout.LabelField($"Enemigos Restantes: {waveManager.EnemiesRemaining}");

            if (waveManager.oleadasTotales > 0)
            {
                float progreso = (float)(waveManager.CurrentWaveIndex + 1) / waveManager.oleadasTotales;
                Rect rect = GUILayoutUtility.GetRect(200, 20);
                EditorGUI.ProgressBar(rect, progreso, "Progreso");
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void BuscarSpawnPoints()
    {
        GameObject[] spawnObjects = GameObject.FindGameObjectsWithTag("SpawnPoint");

        if (spawnPoints != null)
        {
            spawnPoints.arraySize = spawnObjects.Length;

            for (int i = 0; i < spawnObjects.Length; i++)
            {
                spawnPoints.GetArrayElementAtIndex(i).objectReferenceValue = spawnObjects[i].transform;
            }

            serializedObject.ApplyModifiedProperties();
            Debug.Log($"Encontrados {spawnObjects.Length} puntos de spawn");
        }
    }
}
