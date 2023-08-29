using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _WheelFX : MonoBehaviour
{
    

    [Header("Particle System")]
    [SerializeField]
    private ParticleSystem tireSmoke;  // Partículas de fumaça do pneu
    [SerializeField]
    private WheelCollider wheelCollider;
    private float slipThreshold = 0.4f; // Limite para detectar deslizamento/derrapagem
    [SerializeField]
    private float emitRate = 50f; // Taxa de emissão de partículas
    [SerializeField]
    AudioSource tireSqueal; // Som de pneu derrapando

    void FixedUpdate()
    {
      

        CheckForTireSlip(wheelCollider);
        
    }

    void CheckForTireSlip(WheelCollider wheel)
    {
       
        WheelHit wheelHit;
        var emission = tireSmoke.emission;
        if (wheel.GetGroundHit(out wheelHit))
            {
                float slipAmount = Mathf.Abs(wheelHit.forwardSlip) + Mathf.Abs(wheelHit.sidewaysSlip);

                // Se o deslizamento/derrapagem for maior que um limiar, emitir partículas
                if (slipAmount > slipThreshold)
                {
                    // Ajuste a posição do seu ParticleSystem para corresponder à posição do pneu
                    // (Este exemplo supõe que você tem uma única instância do ParticleSystem.
                    // Se você tem um sistema de partículas separado para cada pneu, você precisaria ajustar este código)
                    tireSmoke.transform.position = wheel.transform.position;

                   
                    emission.rateOverTime = emitRate;
                   
                    tireSqueal.volume = slipAmount;
                    tireSqueal.pitch = Mathf.Clamp01(slipAmount);
                }
                else
                {
                    
                    emission.rateOverTime = 0f;
                   
                    tireSqueal.volume = 0;
                }

        }
        else
        {
           
            emission.rateOverTime = 0f;

            tireSqueal.volume = 0;
        }
    }


}
