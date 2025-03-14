using SingletonManagers;
using UnityEngine;

public class EnemyAI_animation : MonoBehaviour
{
    private Animator _enemyai_animator;
    private string _currentAnimation;
    void Awake()
    {
        _enemyai_animator = GetComponent<Animator>();
    }

    void Start()
    {
        
    }
    void Update()
    {
        
    }
    
    private void PlayAnimation(string newAnimation,float SmoothFrame,int WorkingLayer)
    {
        if (_enemyai_animator == null || newAnimation == _currentAnimation) return; 

        _enemyai_animator.CrossFade(newAnimation, SmoothFrame,WorkingLayer);
        _currentAnimation = newAnimation;
        print("death");
    }
}
