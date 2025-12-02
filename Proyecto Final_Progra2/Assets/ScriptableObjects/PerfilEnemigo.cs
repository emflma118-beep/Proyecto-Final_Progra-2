using UnityEngine;

[CreateAssetMenu(fileName = "PerfilEnemigo", menuName = "Wave System/Perfil Enemigo")]
public class PerfilEnemigo : ScriptableObject
{
    [Header("Info Basica")]
    public string nombre;
    public int vidaMax = 100;
    public int daño = 10;
    public int puntos = 25;

    [Header("Configuracion Visual")]
    public GameObject prefab;
    public Material material;
    public Color colorEnemigo = Color.white;

    [Header("Comportamiento")]
    public TipoEnemigo tipoEnemigo = TipoEnemigo.Normalito;
}

public enum TipoEnemigo
{
    Normalito,
    Distancia,
    Velocista,
    Tanque,
    Jefe
}
