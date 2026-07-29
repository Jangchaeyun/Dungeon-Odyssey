using DungeonOdyssey.UI;
using DungeonOdyssey.View3D;
using UnityEngine;

namespace DungeonOdyssey.Core
{
    /// <summary>
    /// Title 씬에 이 컴포넌트만 있으면 분위기 배경·UI를 자동 구성합니다.
    /// </summary>
    public class TitleSceneEntry : MonoBehaviour
    {
        private void Start()
        {
            if (FindFirstObjectByType<TitleAtmosphere3D>() == null)
            {
                TitleAtmosphere3D.Spawn();
            }

            if (FindFirstObjectByType<TitleUI>() == null)
            {
                RuntimeUiFactory.BuildTitleUi();
            }
        }
    }
}
