# SerializableMethodCalls
Serializable Method Calls for Unity. 

Make a `MethodCall` field on a `MonoBehaviour`, add the `MethodCallEditorAttribute` to it, set up your callback in the inspector, and `Invoke()` it.

This package has a few features that separate it from `UnityEvent`s:

- A `UnityEvent` can't return anything. When invoking a Serializable `MethodCall`, however, you will receive the result of the invocation as an object.
- `UnityEvent` can only work with up to one parameter when being set up through the editor. `MethodCall` works with as many parameters as there are, though the editor doesn't handle it extremely gracefully, so try not to exceed 3 or go write some code to make it lay out the parameters nicely.
- A `MethodCall` can resolve its target using a custom resolver.

For more information, see http://blog.preoccupiedgames.com/serialized-method-calls.
