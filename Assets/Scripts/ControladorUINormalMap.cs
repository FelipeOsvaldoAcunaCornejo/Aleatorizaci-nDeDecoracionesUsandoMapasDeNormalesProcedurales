using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Globalization;

public class ControladorUINormalMap : MonoBehaviour
{
    [Header("Referencia al Generador")]
    public GeneradorNormalMap generador;

    [Header("Elementos de la UI")]
    public Slider sliderResolucion;
    public TextMeshProUGUI textoResolucion;
    public TMP_Dropdown dropdownTipoRuido;
    public TMP_InputField inputEscalaRuido;
    public TMP_InputField inputFuerzaNormal;

    void Start()
    {
        if (generador == null) return;

        if (sliderResolucion != null)
        {
            sliderResolucion.minValue = 256;
            sliderResolucion.maxValue = 4096;
            sliderResolucion.wholeNumbers = true;
            sliderResolucion.value = generador.resolucion;
            ActualizarTextoResolucion(generador.resolucion);

            sliderResolucion.onValueChanged.AddListener(OnResolucionCambiada);
        }

        if (dropdownTipoRuido != null)
        {
            dropdownTipoRuido.ClearOptions();
            string[] nombresRuido = Enum.GetNames(typeof(GeneradorNormalMap.TipoRuido));
            dropdownTipoRuido.AddOptions(new List<string>(nombresRuido));

            dropdownTipoRuido.value = (int)generador.tipoRuido;
            dropdownTipoRuido.onValueChanged.AddListener(OnTipoRuidoCambiado);
        }

        if (inputEscalaRuido != null)
        {
            inputEscalaRuido.contentType = TMP_InputField.ContentType.DecimalNumber;
            inputEscalaRuido.text = generador.escalaRuido.ToString(CultureInfo.InvariantCulture);

            inputEscalaRuido.onEndEdit.AddListener(OnEscalaRuidoCambiada);
        }

        if (inputFuerzaNormal != null)
        {
            inputFuerzaNormal.contentType = TMP_InputField.ContentType.DecimalNumber;
            inputFuerzaNormal.text = generador.fuerzaNormal.ToString(CultureInfo.InvariantCulture);

            inputFuerzaNormal.onEndEdit.AddListener(OnFuerzaNormalCambiada);
        }
    }


    public void OnResolucionCambiada(float valor)
    {
        int nuevaResolucion = Mathf.RoundToInt(valor);
        generador.resolucion = nuevaResolucion;
        ActualizarTextoResolucion(nuevaResolucion);
    }

    private void ActualizarTextoResolucion(int valor)
    {
        if (textoResolucion != null)
        {
            textoResolucion.text = $"{valor}x{valor}";
        }
    }

    public void OnTipoRuidoCambiado(int indice)
    {
        generador.tipoRuido = (GeneradorNormalMap.TipoRuido)indice;
    }

    public void OnEscalaRuidoCambiada(string texto)
    {
        if (float.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out float nuevaEscala))
        {
            generador.escalaRuido = Mathf.Max(0.01f, nuevaEscala); // Evitar valores negativos o cero
        }
        // Restaura el texto formateado en caso de que el usuario escribiera algo inválido
        inputEscalaRuido.text = generador.escalaRuido.ToString(CultureInfo.InvariantCulture);
    }

    public void OnFuerzaNormalCambiada(string texto)
    {
        if (float.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out float nuevaFuerza))
        {
            generador.fuerzaNormal = nuevaFuerza;
        }
        inputFuerzaNormal.text = generador.fuerzaNormal.ToString(CultureInfo.InvariantCulture);
    }
}