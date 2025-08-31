export interface UploadResult {
  success: boolean;
  message: string;
  data: string | null;
}

export const uploadFile = async (file: File, endpoint: string): Promise<UploadResult> => {
  const formData = new FormData();
  formData.append('file', file);
  
  try {
    const response = await fetch(endpoint, {
      method: 'POST',
      mode: 'cors',
      headers: {
        'Access-Control-Allow-Origin': '*',
      },
      body: formData,
    });
    
    if (response.ok) {
      const responseData = await response.text();
      return {
        success: true,
        message: 'File uploaded successfully!' + responseData,
        data: responseData
      };
    } else {
      const errorData = await response.text();
      console.log('Error response data:', errorData);
      return {
        success: false,
        message: 'Upload failed: ' + errorData,
        data: errorData
      };
    }
  } catch (error) {
    console.log('Upload error:', error);
    return {
      success: false,
      message: 'Error: ' + (error as Error).message,
      data: null
    };
  }
};
