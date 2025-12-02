using UnityEngine;
using System.Collections;

public abstract class BaseEnemigo : MonoBehaviour, IDamageable
{
    [Header("Componentes")]
    protected Renderer render;

    protected PerfilEnemigo perfil;
    protected int vidaActual;
    protected WaveManager waveManager;

    protected EnemyState estadoActual = EnemyState.Spawning;

    public virtual void Iniciar(PerfilEnemigo perfilEnemigo, WaveManager manager)
    {
        perfil = perfilEnemigo;
        waveManager = manager;
        vidaActual = perfil.vidaMax;

        render = GetComponentInChildren<Renderer>();

        if (render != null)
        {
            if (perfil.material != null)
            {
                render.material = perfil.material;
            }

            render.material.color = perfil.colorEnemigo;
        }

        SetState(EnemyState.Vivo);
    }

    protected virtual void Update()
    {
        if (estadoActual == EnemyState.Muerte)
        {
           
        }
    }

    public virtual void TomarDaño(int daño)
    {
        if (estadoActual == EnemyState.Muerte) return;

        vidaActual -= daño;

        if (vidaActual <= 0)
        {
            Morir();
        }
        else
        {
            StartCoroutine(EfectoDaño());
        }
    }

    protected virtual void Morir()
    {
        SetState(EnemyState.Muerte);
        waveManager?.EnemigoDerrotado(gameObject);

        StartCoroutine(SecuenciaMuerte());
    }

    protected virtual IEnumerator SecuenciaMuerte()
    {
        if (render != null)
        {
            for (int i = 0; i < 3; i++)
            {
                render.enabled = false;
                yield return new WaitForSeconds(0.1f);
                render.enabled = true;
                yield return new WaitForSeconds(0.1f);
            }

            Color colorOriginal = render.material.color;
            for (float t = 0; t < 1; t += Time.deltaTime)
            {
                if (render != null)
                {
                    render.material.color = Color.Lerp(colorOriginal, Color.clear, t);
                }
                yield return null;
            }
        }

        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    protected virtual IEnumerator EfectoDaño()
    {
        if (render != null)
        {
            Color colorOriginal = render.material.color;
            render.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            render.material.color = colorOriginal;
        }
    }

    protected void SetState(EnemyState nuevoEstado)
    {
        estadoActual = nuevoEstado;
    }

    public void DestruirDirectamente()
    {
        Destroy(gameObject);
    }
}

public enum EnemyState
{
    Spawning,
    Vivo,
    Muerte
}
