using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Skill
{
    public string name;
    public bool isUnlocked;
    public bool isAvailable;
    public List<Skill> nextSkills;

        
    public Skill(string name)
    {
    // 1. set the name of the skill, 
    // initialize isUnlocked to false, 
    // and create an empty list of nextSkills
    }

    public void Unlock()
    {
        if (!isAvailable)
        {
            // 2. throw an exception if the skill is not available to unlock
        }

        if (isUnlocked)
        {
            // 3. if the skill is already unlocked, log message and return
        }

        // 4. set isUnlocked to true

        // 5. set isAvailable to true for all nextSkills
    }


    public void PrintSkillTree()
    {
        // 6. log the name of the skill, isAvailable, and isUnlocked
        // and call PrintSkillTree() on all nextSkills
    }

    public void PrintSkillTreeHierarchy(string indent)
    {
        // 7. log the name of the skill, isAvailable, and isUnlocked with indentation
        // and call PrintSkillTreeHierarchy() on all nextSkills

    }

}

public class SkillTree
{
    public Skill rootSkill;

    public SkillTree(Skill rootSkill)
    {
        this.rootSkill = rootSkill;
    }
}

