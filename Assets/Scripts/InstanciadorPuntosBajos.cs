using UnityEngine;
using System.Collections.Generic;

public class InstanciadorPuntosBajos : MonoBehaviour
{
    [Header("Referencias")]
    public GeneradorNormalMap generadorNormalMap; 
    public GameObject prefabAInstanciar;

    [Header("Configuración")]
    public float offsetVertical = 0.05f;
    public Vector3 escalaObjeto = Vector3.one;

    private List<GameObject> instanciasActivas = new List<GameObject>();
    private Vector3[] puntosAnteriores = new Vector3[10];

    void Update()
    {
        if (generadorNormalMap == null || prefabAInstanciar == null || generadorNormalMap.objetoVisualizador == null)
            return;

        if (HanCambiadoPuntos(generadorNormalMap.puntosMasBajosLocales))
        {
            ActualizarInstancias(generadorNormalMap.puntosMasBajosLocales);
        }
    }

    bool HanCambiadoPuntos(Vector3[] nuevosPuntos)
    {
        if (nuevosPuntos == null || nuevosPuntos.Length != puntosAnteriores.Length) return true;

        for (int i = 0; i < nuevosPuntos.Length; i++)
        {
            if (nuevosPuntos[i] != puntosAnteriores[i])
            {
                puntosAnteriores = (Vector3[])nuevosPuntos.Clone();
                return true;
            }
        }
        return false;
    }

    void ActualizarInstancias(Vector3[] puntosLocales)
    {
        foreach (GameObject obj in instanciasActivas)
        {
            if (obj != null) Destroy(obj);
        }
        instanciasActivas.Clear();

        Transform padre = generadorNormalMap.objetoVisualizador.transform;
        foreach (Vector3 puntoLocal in puntosLocales)
        {
            Vector3 posicionConOffset = puntoLocal + (Vector3.up * offsetVertical);

            Vector3 posicionMundial = padre.TransformPoint(posicionConOffset);

            GameObject nuevoObj = Instantiate(prefabAInstanciar, posicionMundial, prefabAInstanciar.transform.rotation, padre);
            nuevoObj.transform.localScale = escalaObjeto;

            instanciasActivas.Add(nuevoObj);
        }
    }
}