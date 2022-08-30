using System.Collections;
using UnityEngine;


namespace CubicSystem.CubicPuzzle
{
    public class BoardMatchHelper :MonoBehaviour
    {
        //�ܰ��� ǥ�ø� ���� LineRenderer
        [SerializeField] private LineRenderer lineRenderer;

        //Swipe Animation �� Dummy Object
        [SerializeField] private Transform tfDummyCell;
        [SerializeField] private Transform tfDummyBlock;
        [SerializeField] private SpriteRenderer spriteDummyBlock;

        private void Start()
        {
            lineRenderer.sortingOrder = 6;
        }

        /**
         *  @brief  Swipe Guide Animation
         *  @param  to : �̵� ��ġ
         */
        private IEnumerator AnimateTargetBlock(Vector2 to)
        {
            var from = tfDummyBlock.localPosition;

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
         *  @brief  ���̵� ���� ���� �� ǥ���ϱ�
         *  @param  helpInfo : ���̵� ���� ����
         */
        public void ShowMatchHelper(MatchHelpInfo helpInfo)
        {
            //�� Ȱ��ȭ
            if(helpInfo == null || helpInfo.MatchCount <= 0) {
                gameObject.SetActive(false);
            }
            //Ȱ��ȭ
            else if(!gameObject.activeSelf){
                var vertexList = helpInfo.CalcOutLineVertex();

                //Clear LineRenderer
                lineRenderer.positionCount = 0;

                //Set LineRenderer
                lineRenderer.positionCount = vertexList.Count;
                for(int i = 0; i < vertexList.Count; i++) {
                    lineRenderer.SetPosition(i, vertexList[i]);
                }

                //Dummy ���� ������Ʈ
                tfDummyCell.localPosition = helpInfo.FromBlock.Position;
                tfDummyBlock.localPosition = helpInfo.FromBlock.Position;
                spriteDummyBlock.color = CubicPuzzleUtility.GetMatchColor(helpInfo.FromBlock.Color);

                //�����ֱ�
                gameObject.SetActive(true);
                StartCoroutine(AnimateTargetBlock(helpInfo.ToBlock.Position));
            }
        }
    }
}

