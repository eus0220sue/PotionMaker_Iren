using UnityEditor;
namespace FC_CutsceneSystem
{
    public static class Menu
    {
        [MenuItem("Tools/Cutscene System/Cutscene Editor")]
        public static void InitNodeEditor()
        {
            NodeEditorWindow.InitEditorWindow(); 
        }
        
        [MenuItem("Tools/Cutscene System/Cutscene Database")]
        public static void InitDatabaseWindow()
        {
            DatabaseWindow.InitDatabaseWindow();
        }
        [MenuItem("Tools/Cutscene System/Welcome Window")]
        public static void InitWelcomeWindow()
        {
            WelcomeWindow.InitEditorWindow();
        }
        

    }
}
