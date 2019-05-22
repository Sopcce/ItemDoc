

namespace Sop.Common.Helper.Extensions
{
    /// <summary>
    /// �ṩ��������ת��ѡ���ö��ֵ��
    /// </summary>
    /// <value>
    /// <code>FirstLetterOnly</code>
    /// ֻת��ƴ������ĸ��Ĭ��ת��ȫ��
    /// </value>
    /// <value>
    /// <code>TranslateUnknowWordToInterrogation</code>
    /// ת��δ֪����Ϊ�ʺţ�Ĭ�ϲ�ת��
    /// </value>
    /// <value>
    /// <code>EnableUnicodeLetter</code>
    /// ��������ĸ���������ַ���Ĭ�ϲ�����
    /// </value>
    [System.FlagsAttribute]
    public enum ChinesePinYinOptions
    {
        /// <summary>
        /// ֻת��ƴ������ĸ��Ĭ��ת��ȫ��
        /// </summary>
        FirstLetterOnly = 1,
        /// <summary>
        /// ת��δ֪����Ϊ�ʺţ�Ĭ�ϲ�ת��
        /// </summary>
        TranslateUnknowWordToInterrogation = 1 << 1,
        /// <summary>
        /// ��������ĸ���������ַ���Ĭ�ϲ�����
        /// </summary>
        EnableUnicodeLetter = 1 << 2,							 
    }

}
