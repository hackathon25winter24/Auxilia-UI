using UnityEngine;
using UnityEngine.UI;
using Game.Network; // ���Ȃ���Namespace
using System.Threading.Tasks;

public class DemoAccountCreator : MonoBehaviour
{
    // ��قǍ���� GameConnector ���C���X�y�N�^�[����h���b�O���h���b�v���܂�
    public GameConnector connector;

    // ���s���ʂ�\�����邽�߂�UI�i����Ε֗��j
    public Text resultText;

    // --- ������{�^���́uOnClick�v�ɐݒ肵�܂� ---
    public async void OnClick_CreateDemoUser()
    {
        Debug.Log("�f�����[�U�[�쐬���N�G�X�g�J�n...");

        // 1. ���M����f���f�[�^��p��
        string demoName = "DemoUser_" + Random.Range(1000, 9999);

        // 2. GameConnector��ʂ��ăT�[�o�[�ɑ��M
        // �iGameConnector�� SignUp ���\�b�h������O��ł��j
        UserResponse response = await connector.SignUp(demoName, "demo_password");

        if (response != null)
        {
            // 3. ����������ID��\��
            string msg = $"�쐬�����I\nID: {response.Id}";
            Debug.Log("<color=yellow>" + msg + "</color>");

            if (resultText != null) resultText.text = msg;

            // �d�v�F��� GetUser �Ŏg����悤��ID�����O���烁�����邩�A
            // PlayerPrefs�i�����ۑ��j���m�F���Ă�������
        }
        else
        {
            Debug.LogError("�쐬�Ɏ��s���܂����B�T�[�o�[�Ƃ̐ڑ����m�F���Ă��������B");
        }
    }
}