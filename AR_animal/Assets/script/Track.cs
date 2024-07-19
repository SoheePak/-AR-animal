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
    // list는 배열
  
    private Dictionary<string, GameObject> dict1 = new Dictionary<string, GameObject>();
    //Dictionary 키와 값을 저장하는 배열
    //c++에서 map과 같은 개념
    private float hideDelay = 0.5f;
    private Dictionary<string, float> lastSeenTime = new Dictionary<string, float>();
    public GameObject animal;
    private int close;

    // Start is called before the first frame update
    void Start()
    {
        foreach(GameObject o in list1)
        {//list1에 있는 배열을 하나씩 꺼내서 GameObject인 o라는 변수에 하나씩 넣는다.
            dict1.Add(o.name, o);
        }
    }
    void UpdateImage(ARTrackedImage t)
    {
        string name = t.referenceImage.name;
        //가지고 오는 애의 이름을 name에 넣는다.
        if(dict1.TryGetValue(name, out GameObject o))
        {
            animal = o;
            /*o.transform.position = t.transform.position;
            // t의 이미지가 있는 위치를 받아 게임 오브젝트 위에 켜진다는 뜻...
            o.transform.rotation = t.transform.rotation;
            // 이미지의 방향대로 오브젝트를 움직임.*/ 
            if (t.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                o.transform.position = t.transform.position;
                // t의 이미지가 있는 위치를 받아 게임 오브젝트 위에 켜진다는 뜻...
                o.transform.rotation = t.transform.rotation;
                // 이미지의 방향대로 오브젝트를 움직임. 
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
        {//add에 있는 정보를 하나씩 꺼내서 t에 넣는다
            UpdateImage(t); //이미지 감지가 될때 added에 먼저감.
            UpdateSound(t);
        }  
        foreach(ARTrackedImage t in args.updated)
        {
            UpdateImage(t);// 이미지의 위치가 바뀌였을 때 불림.
        }
    }
    void OnEnable()
    {// 어디 위치에 있던 상관 없음. start에 있어도 된다.
        manager.trackedImagesChanged += OnChanged;
        //trackedImagesChanged  - 액션타입
        //이미지 변동이(없어지거나, 없거나) 생기면 자동실행하는 함수 = OnChanged
        // +=를 한 이유는 계속 반복해서 실행할 수 있게 해주기 위해
        // 이미지가 바뀔 때에는 OnChanged를 꼭 실행해라.
    }
    void OnDisable()
    {
        manager.trackedImagesChanged -= OnChanged;
    }
    // Update is called once per frame
}
