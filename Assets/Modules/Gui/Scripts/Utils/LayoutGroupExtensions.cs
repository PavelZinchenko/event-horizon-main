using System;
using System.Collections.Generic;
using Services.ObjectPool;
using UnityEngine;
using UnityEngine.UI;

public static class LayoutGroupExtensions
{
    public static int InitializeElements<ViewModelType, ModelType>(
        this Transform targetTransform,
        IEnumerable<ModelType> data,
        Action<ViewModelType, ModelType> Initializer,
        IGameObjectFactory factory)
        where ViewModelType : Component
    {
        int count = 0;
        var enumerator = data.GetEnumerator();
        ViewModelType item = null;
        foreach (Transform transform in targetTransform)
        {
            var viewModel = transform.GetComponent<ViewModelType>();
            if (viewModel == null)
                continue;
            item = viewModel;
            if (enumerator.MoveNext())
            {
                item.gameObject.SetActive(true);
                Initializer(item, enumerator.Current);
                count++;
            }
            else
                item.gameObject.SetActive(false);
        }

        if (item == null)
            return count;

        var parent = item.transform.parent;
        var index = item.transform.GetSiblingIndex();

        while (enumerator.MoveNext())
        {
            var newItem = factory.Create(item.gameObject).GetComponent<ViewModelType>();
            var rectTransform = newItem.GetComponent<RectTransform>();
            rectTransform.SetParent(parent);
            rectTransform.localScale = Vector3.one;
            rectTransform.SetSiblingIndex(++index);
            newItem.gameObject.SetActive(true);
            Initializer(newItem, enumerator.Current);
            count++;
        }

        return count;
    }

    public static int InitializeElements<ViewModelType, ModelType>(
        this Transform targetTransform,
        IEnumerable<ModelType> data,
        Action<ViewModelType, ModelType> Initializer,
        bool explicitType = false)
        where ViewModelType : Component
    {
        int count = 0;
        var enumerator = data.GetEnumerator();
        ViewModelType item = null;
        var noMoreItems = false;
        foreach (Transform transform in targetTransform)
        {
            var viewModel = transform.GetComponent<ViewModelType>();
            if (viewModel == null)
                continue;
            if (explicitType && viewModel.GetType() != typeof(ViewModelType))
                continue;
            item = viewModel;
            noMoreItems = noMoreItems || !enumerator.MoveNext();
            if (noMoreItems)
            {
                item.gameObject.SetActive(false);
            }
            else
            {
                item.gameObject.SetActive(true);
                Initializer(item, enumerator.Current);
                count++;
            }
        }

        if (item == null || noMoreItems)
            return count;

        var parent = item.transform.parent;
        var index = item.transform.GetSiblingIndex();

        while (enumerator.MoveNext())
        {
            var newItem = (ViewModelType)GameObject.Instantiate(item);
            var rectTransform = newItem.GetComponent<RectTransform>();
            rectTransform.SetParent(parent);
            rectTransform.localScale = Vector3.one;
            rectTransform.SetSiblingIndex(++index);
            newItem.gameObject.SetActive(true);
            Initializer(newItem, enumerator.Current);
            count++;
        }

        return count;
    }
}
