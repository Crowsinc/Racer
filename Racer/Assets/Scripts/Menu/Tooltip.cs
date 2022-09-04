using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Tooltip : MonoBehaviour
{
   public TMP_Text header;
   public TMP_Text body;

   public LayoutElement layout;

   public int characterWrapLimit;

   public RectTransform rectTransform;

   private void Awake()
   {
      rectTransform = GetComponent<RectTransform>();
   }
   public void ShowText(string content, string headerText="")
   {
      if (string.IsNullOrEmpty(headerText))
      {
         header.gameObject.SetActive(false);
      }
      else
      {
         header.gameObject.SetActive(true);
         header.text = headerText;
      }

      body.text = content;
      
      int headerLength = header.text.Length;
      int bodyLength = body.text.Length;
      
      layout.enabled = (headerLength > characterWrapLimit || bodyLength > characterWrapLimit) ? true : false;
   }

   private void Update()
   {
      Vector2 position = Input.mousePosition;

      float pivotX = position.x / Screen.width;
      float pivotY = position.y / Screen.height;


      rectTransform.pivot = new Vector2(pivotX, pivotY);
      transform.position = position;
   }
}
