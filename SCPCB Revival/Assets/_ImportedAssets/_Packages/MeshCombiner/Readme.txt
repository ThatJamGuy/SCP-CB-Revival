/*
[README]
Multi-Material - Static Mesh Combiner
Version 1.0.1
Lylek Games

FOR USE IN THE EDITOR
This script is design to combine multiple meshes into a single mesh.

To do this, first create an empty game object. Then drag and your meshes inside so that they are children of the empty
gameobject. Now simply select the empty game object and press Tools > Combine > Static Meshes. All done! Feel free to
take a look at the components of the once empty game object. You will find that the CombineMeshes script has been added.
You may use this script to revert the empty game object back to normal and reactivate the child meshes.

You can also generate collision data, which will utilize Box and Mesh colliders of the child meshes.

You have the option to Save Mesh Data, or Save Mesh Data & Clean Up. The clean up method will destroy all child objects,
which at this point should no longer be necessary.

It is not ideal to combine meshes and not save the data to your project. You should save the data or disable the combine.

The previous version of this asset is included in "MeshCombiner/Unity Package x.x/multimaterialmeshcombiner_x.x.unitypackage".

CONSOLE LOGS
"Process Aborted!" - Did you recieve this message in the console?
When saving mesh data we utilize the name of the active gameObject. However, when data of the same name already exists we
generate a random id and add it to the file name to avoid overwrite. If the process is aborted it is because we already
atempted to generate an id extension a number of times, but even with the random id these files still already exist.
The solution would be to simply try again, or change the name of the gameobject or destination (path).

VIDEO DEMO
https://www.youtube.com/watch?v=S_fzai8ntN0

I have included a 'RateStaticMeshCombiner' script which will propmpt the you to rate this asset. This prompt
should only ever appear one time, regarless of your choice. If there are any issues with this prompt, please let
me know, or simply delete the script located directly in the CombineMeshes folder. Thanks!

SUPPPORT
I try to make assets as user-friendly as possible; please, by all means, do not hesitate to send me an email if you have any questions or comments!
support@lylekgames.com, or visit http://www.lylekgames.com/contacts

**Please leave a rating and review! Even a small review may help immensely with prioritizing updates.**
(Assets with few and infrequent reviews/ratings tend to have less of a priority and my be late or miss-out on crucial compatibility updates, or even be depricated.)
Thank you! =)

*******************************************************************************************

Website
http://www.lylekgames.com/

Support
http://www.lylekgames.com/contacts
*/
