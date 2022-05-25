# BizTalk.Adapter.WinScp
 BizTalk adapter for SFTP and FTP using WinScp

## Description 
This adapter works for both FTP and SFTP using WinScp. Technically it should also work for the other protocols allowed by WinScp, but only (S)FTP is tested.<br/>

### Special Features 
There are some features not in standard adapter that should be practical.
- Username is added to the Uri. _Meaning you can have multiple receive locations to the same host w.o specifiying remotepath_.<br/>
![image](https://user-images.githubusercontent.com/17280237/170330320-ec2a7450-8be9-4088-8799-5a26d0e67c66.png)
- Specify multiple piped filemasks<br/>
![image](https://user-images.githubusercontent.com/17280237/170330683-2af9322f-0bea-4abc-8299-86cd4d1e4f72.png)
- Specify list of piped subfolders<br/>
![image](https://user-images.githubusercontent.com/17280237/170331040-0f1feb19-3aa5-4b2f-b071-19f872cf1c13.png)
- Exclude specified fileextension
- Add a grace period. _Files must be untouched for a certain time before they are picked_.<br/>
![image](https://user-images.githubusercontent.com/17280237/170331386-a52ddc95-4d45-470c-8e7f-a9df34ab8d11.png)
- Better log possibilities<br/>
![image](https://user-images.githubusercontent.com/17280237/170331696-24bc6f47-3911-47fe-ac1d-4eb2fd64ba67.png)
- Temporary fileextension. Used while transmitting files<br/>
![image](https://user-images.githubusercontent.com/17280237/170332174-fd160470-ee2d-495f-9b8e-5c10e7210430.png)
- Dynamic Mode. You dynamically set filename and folder path(s) by updating OutboundTransportLocation in a sendpipeline. _IsDynamic must also be set to true_.<br/>
![image](https://user-images.githubusercontent.com/17280237/170335193-515e3bd9-e104-42f0-9861-7663180c231b.png)
<br/>A tip is to use https://github.com/fernandodsanjos/BizTalk.PipelineComponents.CustomMacros
- Dynamic folder creation. In Dynamic mode remote folder(s) will be created if they do not already exist.





## Installation
Copy all binaries to the folder <b>C:\Program Files (x86)\Common Files\Microsoft BizTalk\BizTalk.Adapter.WinScp</b>. <br/>
If you want to change location of the binaries make sure to change the path in the WinScp.reg file.

Also copy the files bellow from the Biztalk installation folder into the BizTalk.Adapter.WinScp folder.
<br/>WinSCP.exe
<br/>WinSCPnet.dll

<br/>
Run the WinScp.reg file
<br/>
The adapter can now be added like any other adapter.
