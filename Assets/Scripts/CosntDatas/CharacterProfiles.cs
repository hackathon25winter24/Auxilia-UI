using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "CharacterProfiles", menuName = "Scriptable Objects/CharacterProfiles")]
public class CharacterProfiles : ScriptableObject
{
    [SerializeField] CharacterProfs[] _char_profs;

    public CharacterProfs[] char_profs
    {
        get {  return _char_profs; }
    }
}

[System.Serializable]
public class CharacterProfs
{
    [SerializeField] Sprite _image_mini;
    [SerializeField] Sprite _image_big;
    [SerializeField] Sprite _image_detail;
    [SerializeField] string _name;
    [SerializeField] string _gender;
    [SerializeField] string _age;
    [SerializeField] string _birthday;
    [SerializeField] string _affiliation;
    [SerializeField] string _birthplace;
    [SerializeField] string _blood_type;
    [SerializeField] string _height;
    [SerializeField] string _weight;
    [SerializeField] string _hobby;

    public Sprite image_mini
    {
        get { return _image_mini; }
    }

    public Sprite image_big
    {
        get { return _image_big; }
    }

    public Sprite image_detail
    {
        get { return _image_detail; }
    }

    public string name
    {
        get { return _name; }
    }

    public string gender
    {
        get { return _gender; }
    }

    public string age
    {
        get { return _age; }
    }

    public string birthday
    {
        get { return _birthday; }
    }

    public string blood_type
    {
        get { return _blood_type; }
    }

    public string birthplace
    {
        get { return _birthplace; }
    }

    public string affiliation
    {
        get { return _affiliation; }
    }

    public string height
    {
        get { return _height; }
    }

    public string weight
    {
        get { return _weight; }
    }

    public string hobby
    {
        get { return _hobby; }
    }
}
