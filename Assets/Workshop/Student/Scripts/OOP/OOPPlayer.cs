using System;
using UnityEngine;

namespace Solution
{

    public class OOPPlayer : Character
    {
        public Inventory inventory;
        public ActionHistoryManager actionHistoryManager;

        public bool isAutoMoving = false; // Flag to control auto-movement

        public override void SetUP()
        {
            base.SetUP();
            PrintInfo();
            GetRemainEnergy();
            inventory = GetComponent<Inventory>();

        }

        public void Update()
        {           

            // Manual input is only processed if not in auto-move mode
            if (!isAutoMoving)
            {
                if (Input.GetKeyDown(KeyCode.W))
                {
                    Move(Vector2.up);
                }
                if (Input.GetKeyDown(KeyCode.S))
                {
                    Move(Vector2.down);
                }
                if (Input.GetKeyDown(KeyCode.A))
                {
                    Move(Vector2.left);
                }
                if (Input.GetKeyDown(KeyCode.D))
                {
                    Move(Vector2.right);
                }
                if (Input.GetKeyDown(KeyCode.E))
                {
                    UseFireStorm();
                }
                // Input for Undo (U key)
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    actionHistoryManager.UndoLastMove(this);
                }

                // Input for starting an example auto-move sequence (Q key)
                if (Input.GetKeyDown(KeyCode.Q) && !isAutoMoving)
                {
                    actionHistoryManager.StartAutoMoveSequence(this);
                }
            }
        }
        public override void Move(Vector2 direction)
        {
            base.Move(direction);
            mapGenerator.MoveEnemies();
            // Save the state AFTER the move
            Vector2 newPosition = new Vector2(this.positionX, this.positionY);
        }

        public void UseFireStorm()
        {
            if (inventory.HasItem("FireStorm",1))
            {
                inventory.UseItem("FireStorm",1);
                OOPEnemy[] enemies = UtilitySortEnemies.SortEnemiesByRemainningEnergy1(mapGenerator);
                int count = 3;
                if (count > enemies.Length)
                {
                    count = enemies.Length;
                }
                for (int i = 0; i < count; i++)
                {
                    enemies[i].TakeDamage(10);
                }
            }
            else
            {
                Debug.Log("No FireStorm in inventory");
            }
        }
        
        public void Attack(OOPEnemy _enemy)
        {
            _enemy.TakeDamage(AttackPoint);
            Debug.Log(_enemy.name + " is energy " + _enemy.energy);
        }
        protected override void CheckDead()
        {
            base.CheckDead();
            if (energy <= 0)
            {
                Debug.Log("Player is Dead");
            }
        }

    }

}