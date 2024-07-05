using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MysteryDice.Visual
{
    public class HaloSpin : MonoBehaviour
    {
        void Update()
        {
            transform.rotation = Quaternion.Euler(
                Mathf.Sin(Time.time) * 15f,
                Time.time * 30f % 360f,
                0f
            );
        }
    }

}
