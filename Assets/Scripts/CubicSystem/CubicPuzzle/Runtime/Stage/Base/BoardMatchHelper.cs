using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CubicSystem.CubicPuzzle
{
    /**
     *  @brief  BoardMatchHelper Object
     *          시간이 지나면 매치 가능한 블럭을 표시
     */
    public class BoardMatchHelper :MonoBehaviour
    {
        //외곽선 표시를 위한 LineRenderer
        [SerializeField] private LineRenderer lineRenderer;

        //Swipe Animation 용 Dummy Object
        [SerializeField] private Transform tfDummyCell;
        [SerializeField] private Transform tfDummyBlock;
        
        [SerializeField] private SpriteRenderer spriteDummyCell;
        [SerializeField] private SpriteRenderer spriteDummyBlock;

        [Header("[Sprite]", order = 1)]
        [SerializeField] private Sprite hexCellSprite;
        [SerializeField] private Sprite squareCellSprite;

        private void Start()
        {
            lineRenderer.sortingOrder = 6;
        }

        /**
         *  @brief  Swipe Guide Animation
         *  @param  to : 이동 위치
         */
        private IEnumerator AnimateTargetBlock(Vector2 to)
        {
            Vector2 from = tfDummyBlock.localPosition;

            float speed = 1.5f;

            while(true) {
                for(float t = 0f; t < 1f; t += Time.deltaTime / speed) {
                    tfDummyBlock.localPosition = Vector2.Lerp(from, to, t);
                    yield return null;
                }
                tfDummyBlock.localPosition = from;
            }
        }


        /**
         *  @brief  가이드 정보 구성 후 표시하기
         *  @param  helpInfo : 가이드 관련 정보
         */
        public void ShowMatchHelper(ThreeMatchHelpInfo helpInfo)
        {
            //비 활성화
            if(helpInfo == null || helpInfo.MatchCount <= 0) {
                gameObject.SetActive(false);
            }
            //활성화
            else if(!gameObject.activeSelf){
                List<Vector2> vertexList = helpInfo.CalcOutLineVertex();

                //Clear LineRenderer
                lineRenderer.positionCount = 0;

                //Set LineRenderer
                lineRenderer.positionCount = vertexList.Count;
                for(int i = 0; i < vertexList.Count; i++) {
                    lineRenderer.SetPosition(i, vertexList[i]);
                }

                //Dummy 정보 업데이트
                tfDummyCell.localPosition = helpInfo.FromBlock.Position;
                tfDummyBlock.localPosition = helpInfo.FromBlock.Position;
                spriteDummyBlock.color = CubicPuzzleUtility.GetMatchColor(helpInfo.FromBlock.Color);

                spriteDummyCell.sprite = helpInfo.GetCellStyle() == CellStyle.HEX ? hexCellSprite : squareCellSprite;

                gameObject.SetActive(true);
                StartCoroutine(AnimateTargetBlock(helpInfo.ToBlock.Position));
            }
        }
    }
}

