using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;

public class Track : MonoBehaviour
{
    public ARTrackedImageManager manager; 
    public List<GameObject> list1 = new List<GameObject>();
    // list�� �迭
  
    private Dictionary<string, GameObject> dict1 = new Dictionary<string, GameObject>();
    //Dictionary Ű�� ���� �����ϴ� �迭
    //c++���� map�� ���� ����
    private float hideDelay = 0.5f;
    private Dictionary<string, float> lastSeenTime = new Dictionary<string, float>();
    public GameObject animal;
    private int close;

    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject o in list1)
        {//list1�� �ִ� �迭�� �ϳ��� ������ GameObject�� o��� ������ �ϳ��� �ִ´�.
            dict1.Add(o.name, o);
        }
    }
    void UpdateImage(ARTrackedImage t)
    {
        string name = t.referenceImage.name;
        //������ ���� ���� �̸��� name�� �ִ´�.
        if(dict1.TryGetValue(name, out GameObject o))
        {
            animal = o;
            /*o.transform.position = t.transform.position;
            // t�� �̹����� �ִ� ��ġ�� �޾� ���� ������Ʈ ���� �����ٴ� ��...
            o.transform.rotation = t.transform.rotation;
            // �̹����� ������ ������Ʈ�� ������.*/ 
            if (t.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                o.transform.position = t.transform.position;
                // t�� �̹����� �ִ� ��ġ�� �޾� ���� ������Ʈ ���� �����ٴ� ��...
                o.transform.rotation = t.transform.rotation;
                // �̹����� ������ ������Ʈ�� ������. 
                o.SetActive(true);
                close = 0;
            }
            else
            {
                if (close == 1)
                {
                    o.SetActive(false);
                }
            }

        }
    }
    public void other()
    {
        animal.SetActive(false);
        close = 1;
    }


    void UpdateSound(ARTrackedImage t)
    {
        string name = t.referenceImage.name;
    }
    private void OnChanged(ARTrackedImagesChangedEventArgs args)
    { 
        foreach(ARTrackedImage t in args.added)
        {//add�� �ִ� ������ �ϳ��� ������ t�� �ִ´�
            UpdateImage(t); //�̹��� ������ �ɶ� added�� ������.
            UpdateSound(t);
        }  
        foreach(ARTrackedImage t in args.updated)
        {
            UpdateImage(t);// �̹����� ��ġ�� �ٲ�� �� �Ҹ�.
        }
    }
    void OnEnable()
    {// ��� ��ġ�� �ִ� ��� ����. start�� �־ �ȴ�.
        manager.trackedImagesChanged += OnChanged;
        //trackedImagesChanged  - �׼�Ÿ��
        //�̹��� ������(�������ų�, ���ų�) ����� �ڵ������ϴ� �Լ� = OnChanged
        // +=�� �� ������ ��� �ݺ��ؼ� ������ �� �ְ� ���ֱ� ����
        // �̹����� �ٲ� ������ OnChanged�� �� �����ض�.
    }
    void OnDisable()
    {
        manager.trackedImagesChanged -= OnChanged;
    }
    // Update is called once per frame
}
