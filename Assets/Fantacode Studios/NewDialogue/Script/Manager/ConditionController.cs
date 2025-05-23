using System;

public static class ConditionController
{
    public static bool IsMet(string key, string expectedValue)
    {
        if (string.IsNullOrEmpty(key)) return false;

        if (key.StartsWith("Quest."))
        {
            string questId = key.Split('.')[1];
            //return QuestManager.GetQuestState(questId) == expectedValue;
        }

        if (key.StartsWith("Item."))
        {
            string itemId = key.Split('.')[1];
            bool shouldHave = bool.Parse(expectedValue);

            return GManager.Instance.IsinvenManager.HasItemById(itemId) == shouldHave;
        }


        return false;
    }
}
