using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDebugger : PersistentSingleton<SceneDebugger>
{
   [SerializeField] private string m_gameSceneName;
   [SerializeField] private string m_menuSceneName;
   [SerializeField] private string m_testSceneName;
   
   
   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.F1))
      {
         LoadScene(m_menuSceneName);
      }
       
    
      if (Input.GetKeyDown(KeyCode.F2))
      {
         LoadScene(m_gameSceneName);
      }
 
      if (Input.GetKeyDown(KeyCode.F3))
      {
         LoadScene(m_testSceneName);
      }
   }

   private void LoadScene(string sceneName)
   {
      if (!string.IsNullOrEmpty(sceneName))
      {
         SceneManager.LoadScene(sceneName);
      }
  
   }

}
