using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Solution
{

    public class OOPWall : Identity
    {
        public int Damage;
        public bool IsIceWall;

        private void Start()
        {
            IsIceWall = Random.Range(0, 100) < 20 ? true : false;
            if (IsIceWall)
            {
                GetComponent<SpriteRenderer>().color = Color.blue;
            }
        }
        public override bool Hit()
        {
            if (IsIceWall)
            {
                mapGenerator.player.TakeDamage(Damage, IsIceWall);
            }
            else
            {
                mapGenerator.player.TakeDamage(Damage);
            }
            mapGenerator.mapdata[positionX, positionY] = null;
            Destroy(gameObject);
            return false;
        }
    }
}