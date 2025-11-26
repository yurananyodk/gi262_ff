using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class SkillBook : MonoBehaviour
    {
        public SkillTree attackSkillTree;

        Skill attack = new Skill("attack");
        Skill fireStorm = new Skill("fireStorm");
        Skill fireBall = new Skill("fireBall");
        Skill fireBlast = new Skill("fireBlast");
        Skill fireWave= new Skill("fireWave");
        Skill fireExplosion = new Skill("fireExplosion");

        public void Start()
        {
        // build skill tree
        // └── Attack
        //     └── FireStorm
        //         ├── FireBlast
        //         └── FireBall
        //             └── FireWave
        //                 └── FireExplosion

        // [0] Attack -> FireStorm

        // [1] FireStorm -> FireBlast

        // [2] FireStorm -> FireBall

        // [3] FireBall -> FireWave

        // [4] FireWave -> FireExplosion

        // [5] Attack -> FireStorm

        this.attackSkillTree = new SkillTree(attack);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                attackSkillTree.rootSkill.PrintSkillTreeHierarchy("");
                // attackSkillTree.rootSkill.PrintSkillTree();
                Debug.Log("====================================");
            } 
        }
    }

