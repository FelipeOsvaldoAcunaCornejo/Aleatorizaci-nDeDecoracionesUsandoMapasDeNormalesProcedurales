using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GeneradorNormalMap : MonoBehaviour
{
    public enum TipoRuido
    {
        Perlin,
        Senoidal,
        Concreto,
        OlasCombinadas
    }

    [Header("Configuración de Ruido")]
    public int resolucion = 512;
    int resolucionActual = 0;
    public TipoRuido tipoRuido = TipoRuido.Perlin;
    TipoRuido tipoRuidoActual;
    public float escalaRuido = 20f;
    float escalaRuidoActual = 0f;
    public float fuerzaNormal = 2f;
    float fuerzaNormalActual = 0f;

    [Header("Resultado")]
    public Renderer objetoVisualizador;

    [Header("Configuración de Puntos Bajos")]
    [Tooltip("Distancia mínima en píxeles UV (0 a 1) para evitar que los puntos estén muy pegados.")]
    public float distanciaMinimaEntrePuntos = 0.05f;

    [Header("Depuración: 10 Puntos Más Bajos")]
    [SerializeField] public Vector3[] puntosMasBajosLocales = new Vector3[10];

    void FixedUpdate()
    {
        if (resolucion != resolucionActual || tipoRuido != tipoRuidoActual || escalaRuido != escalaRuidoActual || fuerzaNormal != fuerzaNormalActual)
        {
            Texture2D mapaDeNormales = GeneradorNormalMapProcedural();
            resolucionActual = resolucion;
            tipoRuidoActual = tipoRuido;
            escalaRuidoActual = escalaRuido;
            fuerzaNormalActual = fuerzaNormal;

            if (objetoVisualizador != null)
            {
                objetoVisualizador.material.SetTexture("_BumpMap", mapaDeNormales);
                objetoVisualizador.material.EnableKeyword("_NORMALMAP");
            }
        }
    }

    Texture2D GeneradorNormalMapProcedural()
    {
        Texture2D normalTexture = new Texture2D(resolucion, resolucion, TextureFormat.RGBA32, true);
        Color[] pixeles = new Color[resolucion * resolucion];
        float[,] mapaAlturas = new float[resolucion, resolucion];
        for (int x = 0; x < resolucion; x++)
        {
            for (int y = 0; y < resolucion; y++)
            {
                float xCoord = (float)x / resolucion * escalaRuido;
                float yCoord = (float)y / resolucion * escalaRuido;

                switch (tipoRuido)
                {
                    case TipoRuido.Perlin:
                        mapaAlturas[x, y] = Mathf.PerlinNoise(xCoord, yCoord);
                        break;
                    case TipoRuido.Senoidal:
                        mapaAlturas[x, y] = (Mathf.Sin(xCoord * Mathf.PI) + Mathf.Cos(yCoord * Mathf.PI) + 2f) * 0.25f;
                        break;
                    case TipoRuido.OlasCombinadas:
                        mapaAlturas[x, y] = Mathf.Sin(xCoord) * Mathf.Cos(yCoord) * 0.5f + 0.5f;
                        break;
                    case TipoRuido.Concreto:
                        float baseConcreto = Mathf.PerlinNoise(xCoord, yCoord);
                        float granoFino = Mathf.PerlinNoise(xCoord * 4f, yCoord * 4f);
                        float microDetalle = Mathf.PerlinNoise(xCoord * 12f, yCoord * 12f);
                        float ruidoConcreto = (baseConcreto * 0.6f) + (granoFino * 0.3f) + (microDetalle * 0.1f);
                        mapaAlturas[x, y] = Mathf.Pow(ruidoConcreto, 1.2f);
                        break;

                }
            }
        }
        DetectarYTraducirPuntosBajos(mapaAlturas);
        for (int x = 0; x < resolucion; x++)
        {
            for (int y = 0; y < resolucion; y++)
            {
                float altoIzquierda = mapaAlturas[(x - 1 + resolucion) % resolucion, y];
                float altoDerecha = mapaAlturas[(x + 1) % resolucion, y];
                float altoAbajo = mapaAlturas[x, (y - 1 + resolucion) % resolucion];
                float altoArriba = mapaAlturas[x, (y + 1) % resolucion];

                float dX = (altoIzquierda - altoDerecha) * fuerzaNormal;
                float dY = (altoAbajo - altoArriba) * fuerzaNormal;

                Vector3 normal = new Vector3(dX, dY, 1.0f).normalized;
                float r = (normal.x + 1f) * 0.5f;
                float g = (normal.y + 1f) * 0.5f;
                float b = (normal.z + 1f) * 0.5f;

                pixeles[y * resolucion + x] = new Color(r, g, b, 1f);
            }
        }

        normalTexture.SetPixels(pixeles);
        normalTexture.Apply();
        return normalTexture;
    }

    void DetectarYTraducirPuntosBajos(float[,] mapaAlturas)
    {
        List<PuntoAltura> listaPuntos = new List<PuntoAltura>();

        for (int x = 0; x < resolucion; x++)
        {
            for (int y = 0; y < resolucion; y++)
            {
                listaPuntos.Add(new PuntoAltura { x = x, y = y, altura = mapaAlturas[x, y] });
            }
        }

        // Ordenamos de menor a mayor altura
        var puntosOrdenados = listaPuntos.OrderBy(p => p.altura).ToList();
        List<PuntoAltura> puntosFiltrados = new List<PuntoAltura>();

        // Aplicamos filtro de distancia mínima (en espacio UV de 0.0 a 1.0)
        foreach (var punto in puntosOrdenados)
        {
            float uActual = (float)punto.x / (resolucion - 1);
            float vActual = (float)punto.y / (resolucion - 1);

            bool muyCerca = false;
            foreach (var elegido in puntosFiltrados)
            {
                float uElegido = (float)elegido.x / (resolucion - 1);
                float vElegido = (float)elegido.y / (resolucion - 1);

                // Calculamos la distancia euclidiana en el plano UV
                float distancia = Mathf.Sqrt(Mathf.Pow(uActual - uElegido, 2) + Mathf.Pow(vActual - vElegido, 2));

                if (distancia < distanciaMinimaEntrePuntos)
                {
                    muyCerca = true;
                    break;
                }
            }

            // Si pasa el filtro de distancia, lo añadimos
            if (!muyCerca)
            {
                puntosFiltrados.Add(punto);
            }

            // Paramos cuando ya tengamos los 10 puntos válidos y separados
            if (puntosFiltrados.Count >= 10)
            {
                break;
            }
        }

        puntosMasBajosLocales = new Vector3[10];

        // Obtenemos los límites del objeto visualizador para hacer la conversión proporcional
        Bounds bounds = new Bounds(Vector3.zero, new Vector3(10f, 0f, 10f));
        if (objetoVisualizador != null)
        {
            MeshFilter mf = objetoVisualizador.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null)
            {
                bounds = mf.sharedMesh.bounds;
            }
            else
            {
                bounds = objetoVisualizador.bounds;
            }
        }

        for (int i = 0; i < puntosFiltrados.Count; i++)
        {
            var p = puntosFiltrados[i];

            float u = (float)p.x / (resolucion - 1);
            float v = (float)p.y / (resolucion - 1);

            float localX = Mathf.Lerp(bounds.min.x, bounds.max.x, u);
            float localZ = Mathf.Lerp(bounds.min.z, bounds.max.z, v);
            float localY = p.altura;

            puntosMasBajosLocales[i] = new Vector3(localX, localY, localZ);
        }
    }

    private struct PuntoAltura
    {
        public int x;
        public int y;
        public float altura;
    }
}