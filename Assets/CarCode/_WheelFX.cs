using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _WheelFX : MonoBehaviour
{
    

    [Header("Particle System")]
    [SerializeField]
    private ParticleSystem tireSmoke;  // Part�culas de fuma�a do pneu
    [SerializeField]
    private WheelCollider wheelCollider;
    private float slipThreshold = 0.4f; // Limite para detectar deslizamento/derrapagem
    [SerializeField]
    private float emitRate = 50f; // Taxa de emiss�o de part�culas
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

                // Se o deslizamento/derrapagem for maior que um limiar, emitir part�culas
                if (slipAmount > slipThreshold)
                {
                    // Ajuste a posi��o do seu ParticleSystem para corresponder � posi��o do pneu
                    // (Este exemplo sup�e que voc� tem uma �nica inst�ncia do ParticleSystem.
                    // Se voc� tem um sistema de part�culas separado para cada pneu, voc� precisaria ajustar este c�digo)
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
