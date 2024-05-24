using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using TUCAMAPI;

namespace C.NET11_trigger_hardware
{
    class Program
    {
        public static TUCAM_INIT m_itApi;       // SDK API initialized object
        public static TUCAM_OPEN m_opCam;       // Open camera object
        public static TUCAM_FRAME m_frame;      // The frame object
        public static TUCAM_TRIGGER_ATTR m_tgr; // The trigger object

        /* Init the TUCAM API */
        static TUCAMRET InitApi()
        {
            /* Get the current directory */
            IntPtr strPath = Marshal.StringToHGlobalAnsi(System.Environment.CurrentDirectory);

            m_itApi.uiCamCount = 0;
            m_itApi.pstrConfigPath = strPath;

            TUCamAPI.TUCAM_Api_Init(ref m_itApi);

            Console.WriteLine("Connect {0} camera", m_itApi.uiCamCount);

            if (0 == m_itApi.uiCamCount)
            {
                return TUCAMRET.TUCAMRET_NO_CAMERA;
            }

            return TUCAMRET.TUCAMRET_SUCCESS;
        }

        /* UnInit the TUCAM API */
        static TUCAMRET UnInitApi()
        {
            return TUCamAPI.TUCAM_Api_Uninit();
        }

        /* Open the camera by index number */
        static TUCAMRET OpenCamera(uint uiIdx)
        {
            if (uiIdx >= m_itApi.uiCamCount)
            {
                return TUCAMRET.TUCAMRET_OUT_OF_RANGE;
            }

            m_opCam.uiIdxOpen = uiIdx;

            return TUCamAPI.TUCAM_Dev_Open(ref m_opCam);
        }

        /* Close the current camera */
        static TUCAMRET CloseCamera()
        {
            if (null != m_opCam.hIdxTUCam)
            {
                TUCamAPI.TUCAM_Dev_Close(m_opCam.hIdxTUCam);
            }

            Console.WriteLine("Close the camera success");

            return TUCAMRET.TUCAMRET_SUCCESS;
        }

        /* Do hardware trigger */
        static void DoHardwareTrigger()
        {
            int nTimes = 10;

            m_frame.pBuffer = IntPtr.Zero;
            m_frame.ucFormatGet = (byte)TUFRM_FORMATS.TUFRM_FMT_USUAl;
            m_frame.uiRsdSize = 1;

            TUCamAPI.TUCAM_Cap_GetTrigger(m_opCam.hIdxTUCam, ref m_tgr);
            m_tgr.nTgrMode = (int)TUCAM_CAPTURE_MODES.TUCCM_TRIGGER_STANDARD;
            /* how many frames do you want to capture to RAM(the frames less than 0, use maximum frames ) */
            m_tgr.nFrames = 1;
            m_tgr.nBufFrames = 1;
            m_tgr.nDelayTm = 0;
            m_tgr.nExpMode = (int)TUCAM_TRIGGER_EXP.TUCTE_EXPTM;
            m_tgr.nEdgeMode = (int)TUCAM_TRIGGER_EDGE.TUCTD_RISING;
            TUCamAPI.TUCAM_Cap_SetTrigger(m_opCam.hIdxTUCam, m_tgr);

            TUCamAPI.TUCAM_Buf_Alloc(m_opCam.hIdxTUCam, ref m_frame);

            if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Cap_Start(m_opCam.hIdxTUCam, (uint)TUCAM_CAPTURE_MODES.TUCCM_TRIGGER_STANDARD))
            {
                for (int i = 0; i < nTimes; ++i)
                {
                    if (TUCAMRET.TUCAMRET_SUCCESS == TUCamAPI.TUCAM_Buf_WaitForFrame(m_opCam.hIdxTUCam, ref m_frame))
                    {
                        /* After call TUCAM_Buf_WaitForFrame your hardware can send the trigger signal */
                        /* if m_tgr.nFrames == 1, the SDK create 1 frame memory to receive one trigger signal and the TUCAM_Buf_WaitForFrame interface return once */
                        /* if m_tgr.nFrames == N, the SDK create N frames memory to receive N times trigger signals, the TUCAM_Buf_WaitForFrame interface return, and you call TUCAM_Buf_WaitForFrame N-1 times to grab the frame */
                        /* if m_tgr.nFrames < 0, the SDK create Max frames memory to receive trigger signals */

                        Console.WriteLine("Grab the hardware trigger frame success, index number is {0}, width:{1}, height:{2}, channel:{3}, depth:{4}, image size:{5}", i, m_frame.usWidth, m_frame.usHeight, m_frame.ucChannels, (2 == m_frame.ucElemBytes) ? 16 : 8, m_frame.uiImgSize);
                    }
                    else
                    {
                        Console.WriteLine("Grab the hardware trigger frame failure, index number is {0}", i);
                    }
                }

                TUCamAPI.TUCAM_Buf_AbortWait(m_opCam.hIdxTUCam);
                TUCamAPI.TUCAM_Cap_Stop(m_opCam.hIdxTUCam);
            }

            TUCamAPI.TUCAM_Buf_Release(m_opCam.hIdxTUCam);
        }

        static void Main(string[] args)
        {
            if (TUCAMRET.TUCAMRET_SUCCESS == InitApi())
            {
                if (TUCAMRET.TUCAMRET_SUCCESS == OpenCamera(0))
                {
                    Console.WriteLine("Open the camera success");

                    Console.WriteLine();
                    DoHardwareTrigger();
                    Console.WriteLine();

                    CloseCamera();
                }
                else
                {
                    Console.WriteLine("Open the camera failure");
                }

                UnInitApi();
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
