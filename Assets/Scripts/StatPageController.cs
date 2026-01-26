using UnityEngine;

public class StatPageController : MonoBehaviour
{
    [SerializeField] private GameObject page1Panel;
    [SerializeField] private GameObject page2Panel;

    private int currentPage = 1;

    public void GoToNextPage()
    {
        if (currentPage == 1)
        {
            page1Panel.SetActive(false);
            page2Panel.SetActive(true);
            currentPage = 2;
        }
    }

    public void GoToPreviousPage()
    {
        if (currentPage == 2)
        {
            page1Panel.SetActive(true);
            page2Panel.SetActive(false);
            currentPage = 1;
        }
    }
}
